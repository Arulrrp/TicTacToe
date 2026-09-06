import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Scoreboard } from '../../models/game.model';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.scss'
})
export class ScoreboardComponent {
  @Input() scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  @Output() resetScoreboard = new EventEmitter<void>();
}
