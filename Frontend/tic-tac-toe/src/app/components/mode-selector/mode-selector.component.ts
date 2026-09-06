import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameMode } from '../../models/game.model';

@Component({
  selector: 'app-mode-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mode-selector.component.html',
  styleUrl: './mode-selector.component.scss'
})
export class ModeSelectorComponent {
  @Input() mode: GameMode = 'TwoPlayer';
  @Output() modeChange = new EventEmitter<GameMode>();

  select(mode: GameMode): void {
    if (mode !== this.mode) {
      this.modeChange.emit(mode);
    }
  }
}
