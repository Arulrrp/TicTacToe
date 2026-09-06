import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GameService } from './game.service';
import { environment } from '../../environments/environment';
import { GameState } from '../models/game.model';

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;

  const sampleState: GameState = {
    gameId: 'abc123',
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

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GameService]
    });
    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('creates a game with the requested mode', () => {
    service.createGame('VsComputer').subscribe((state) => {
      expect(state.gameId).toBe('abc123');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/games`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush(sampleState);
  });

  it('submits a move with player and cellIndex', () => {
    service.submitMove('abc123', 'X', 4).subscribe((state) => {
      expect(state).toEqual(sampleState);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/games/abc123/moves`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 4 });
    req.flush(sampleState);
  });

  it('calls undo endpoint', () => {
    service.undo('abc123').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/games/abc123/undo`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleState);
  });

  it('fetches the scoreboard', () => {
    service.getScoreboard().subscribe((sb) => {
      expect(sb.xWins).toBe(0);
    });
    const req = httpMock.expectOne(`${environment.apiUrl}/scoreboard`);
    expect(req.request.method).toBe('GET');
    req.flush({ xWins: 0, oWins: 0, draws: 0 });
  });

  it('resets the scoreboard', () => {
    service.resetScoreboard().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/scoreboard/reset`);
    expect(req.request.method).toBe('POST');
    req.flush({ xWins: 0, oWins: 0, draws: 0 });
  });
});
