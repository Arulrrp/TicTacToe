import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { GameState } from './models/game.model';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let component: AppComponent;
  let httpMock: HttpTestingController;

  const baseState: GameState = {
    gameId: 'g1',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    canUndo: false,
    scoreboard: { xWins: 0, oWins: 0, draws: 0 }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function createInitialGame(): void {
    fixture.detectChanges(); // triggers ngOnInit -> createGame
    const req = httpMock.expectOne(`${environment.apiUrl}/games`);
    req.flush(baseState);
  }

  it('creates a new Two Player game on init', () => {
    createInitialGame();
    expect(component.gameState?.gameId).toBe('g1');
  });

  it('submits a move when a cell is clicked', () => {
    createInitialGame();

    component.onCellClicked(0);
    const req = httpMock.expectOne(`${environment.apiUrl}/games/g1/moves`);
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 0 });

    const updated: GameState = { ...baseState, board: ['X', null, null, null, null, null, null, null, null], currentPlayer: 'O' };
    req.flush(updated);

    expect(component.gameState?.currentPlayer).toBe('O');
  });

  it('shows a friendly error when the backend rejects a move', () => {
    createInitialGame();

    component.onCellClicked(0);
    const req = httpMock.expectOne(`${environment.apiUrl}/games/g1/moves`);
    req.flush({ message: 'That cell is already occupied.' }, { status: 400, statusText: 'Bad Request' });

    expect(component.errorMessage).toBe('That cell is already occupied.');
  });

  it('starts a fresh game when the mode is changed', () => {
    createInitialGame();

    component.onModeChange('VsComputer');
    const req = httpMock.expectOne(`${environment.apiUrl}/games`);
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush({ ...baseState, mode: 'VsComputer' });

    expect(component.gameState?.mode).toBe('VsComputer');
  });

  it('calls undo and updates state', () => {
    createInitialGame();

    component.onUndo();
    const req = httpMock.expectOne(`${environment.apiUrl}/games/g1/undo`);
    req.flush(baseState);

    expect(component.gameState).toBeTruthy();
  });

  it('resets the scoreboard independently of the game board', () => {
    createInitialGame();

    component.onResetScoreboard();
    const req = httpMock.expectOne(`${environment.apiUrl}/scoreboard/reset`);
    req.flush({ xWins: 0, oWins: 0, draws: 0 });

    expect(component.gameState?.scoreboard).toEqual({ xWins: 0, oWins: 0, draws: 0 });
  });
});
