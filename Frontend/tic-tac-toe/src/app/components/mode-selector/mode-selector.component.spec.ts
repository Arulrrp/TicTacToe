import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModeSelectorComponent } from './mode-selector.component';

describe('ModeSelectorComponent', () => {
  let fixture: ComponentFixture<ModeSelectorComponent>;
  let component: ModeSelectorComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModeSelectorComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ModeSelectorComponent);
    component = fixture.componentInstance;
  });

  it('emits modeChange when a different mode is selected', () => {
    component.mode = 'TwoPlayer';
    fixture.detectChanges();
    spyOn(component.modeChange, 'emit');

    const buttons = fixture.nativeElement.querySelectorAll('.mode-btn');
    buttons[1].click();

    expect(component.modeChange.emit).toHaveBeenCalledWith('VsComputer');
  });

  it('does not emit when the already-active mode is clicked again', () => {
    component.mode = 'TwoPlayer';
    fixture.detectChanges();
    spyOn(component.modeChange, 'emit');

    const buttons = fixture.nativeElement.querySelectorAll('.mode-btn');
    buttons[0].click();

    expect(component.modeChange.emit).not.toHaveBeenCalled();
  });
});
