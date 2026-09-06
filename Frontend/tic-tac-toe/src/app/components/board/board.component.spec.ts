import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BoardComponent } from './board.component';

describe('BoardComponent', () => {
  let fixture: ComponentFixture<BoardComponent>;
  let component: BoardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
  });

  it('creates the board with 9 cells', () => {
    fixture.detectChanges();
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    expect(cells.length).toBe(9);
  });

  it('emits cellClicked when an empty, in-progress cell is clicked', () => {
    component.board = Array(9).fill(null);
    component.status = 'InProgress';
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[0].click();

    expect(component.cellClicked.emit).toHaveBeenCalledWith(0);
  });

  it('does not emit for an already-occupied cell', () => {
    component.board = ['X', null, null, null, null, null, null, null, null];
    component.status = 'InProgress';
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[0].click();

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('does not emit once the game has finished', () => {
    component.board = Array(9).fill(null);
    component.status = 'Won';
    fixture.detectChanges();

    spyOn(component.cellClicked, 'emit');
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[3].click();

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('marks winning cells', () => {
    component.board = ['X', 'X', 'X', null, null, null, null, null, null];
    component.winningCells = [0, 1, 2];
    fixture.detectChanges();

    const cells = fixture.nativeElement.querySelectorAll('.cell');
    expect(cells[0].classList).toContain('winning');
    expect(cells[3].classList).not.toContain('winning');
  });
});
