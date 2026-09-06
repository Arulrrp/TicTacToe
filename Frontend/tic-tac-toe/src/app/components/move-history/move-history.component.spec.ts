import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MoveHistoryComponent } from './move-history.component';

describe('MoveHistoryComponent', () => {
  let fixture: ComponentFixture<MoveHistoryComponent>;
  let component: MoveHistoryComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoveHistoryComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MoveHistoryComponent);
    component = fixture.componentInstance;
  });

  it('shows an empty message when there are no moves', () => {
    component.moves = [];
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.empty')).toBeTruthy();
  });

  it('renders one row per move', () => {
    component.moves = [
      { moveNumber: 1, player: 'X', cellIndex: 0, row: 0, col: 0 },
      { moveNumber: 2, player: 'O', cellIndex: 4, row: 1, col: 1 }
    ];
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[1].textContent).toContain('Row 2, Column 2');
  });
});
