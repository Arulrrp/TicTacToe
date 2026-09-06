import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameStatus, PlayerSymbol } from '../../models/game.model';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.component.html',
  styleUrl: './board.component.scss'
})
export class BoardComponent {
  @Input() board: (PlayerSymbol | null)[] = Array(9).fill(null);
  @Input() winningCells: number[] = [];
  @Input() status: GameStatus = 'InProgress';
  @Input() disabled = false;

  @Output() cellClicked = new EventEmitter<number>();

  isWinningCell(index: number): boolean {
    return this.winningCells.includes(index);
  }

  isClickable(index: number): boolean {
    return !this.disabled && this.status === 'InProgress' && this.board[index] === null;
  }

  rowOf(index: number): number {
    return Math.floor(index / 3) + 1;
  }

  colOf(index: number): number {
    return (index % 3) + 1;
  }

  onCellClick(index: number): void {
    if (!this.isClickable(index)) {
      return;
    }
    this.cellClicked.emit(index);
  }
}
