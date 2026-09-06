import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScoreboardComponent } from './scoreboard.component';

describe('ScoreboardComponent', () => {
  let fixture: ComponentFixture<ScoreboardComponent>;
  let component: ScoreboardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScoreboardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ScoreboardComponent);
    component = fixture.componentInstance;
  });

  it('renders the provided scoreboard values', () => {
    component.scoreboard = { xWins: 3, oWins: 1, draws: 2 };
    fixture.detectChanges();

    const values = fixture.nativeElement.querySelectorAll('.value');
    expect(values[0].textContent).toContain('3');
    expect(values[1].textContent).toContain('1');
    expect(values[2].textContent).toContain('2');
  });

  it('emits resetScoreboard when the button is clicked', () => {
    fixture.detectChanges();
    spyOn(component.resetScoreboard, 'emit');

    fixture.nativeElement.querySelector('.reset-btn').click();

    expect(component.resetScoreboard.emit).toHaveBeenCalled();
  });
});
