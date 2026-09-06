import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MoveHistoryItem } from '../../models/game.model';

@Component({
  selector: 'app-move-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './move-history.component.html',
  styleUrl: './move-history.component.scss'
})
export class MoveHistoryComponent {
  @Input() moves: MoveHistoryItem[] = [];
}
