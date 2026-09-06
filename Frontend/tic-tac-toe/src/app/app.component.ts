import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { GameService } from './services/game.service';
import { GameMode, GameState } from './models/game.model';
import { BoardComponent } from './components/board/board.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ModeSelectorComponent } from './components/mode-selector/mode-selector.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    BoardComponent,
    ScoreboardComponent,
    MoveHistoryComponent,
    ModeSelectorComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  gameState: GameState | null = null;
  errorMessage: string | null = null;
  loading = false;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame('TwoPlayer');
  }

  get statusMessage(): string {
    if (!this.gameState) return '';
    if (this.gameState.status === 'Won') {
      return `Player ${this.gameState.winner} wins!`;
    }
    if (this.gameState.status === 'Draw') {
      return "It's a draw!";
    }
    return `Player ${this.gameState.currentPlayer}'s turn`;
  }

  onModeChange(mode: GameMode): void {
    this.startNewGame(mode);
  }

  onCellClicked(cellIndex: number): void {
    if (!this.gameState) return;
    const { gameId, currentPlayer } = this.gameState;

    this.errorMessage = null;
    this.gameService.submitMove(gameId, currentPlayer, cellIndex).subscribe({
      next: (state) => (this.gameState = state),
      error: (err) => this.handleError(err)
    });
  }

  onUndo(): void {
    if (!this.gameState) return;
    this.errorMessage = null;
    this.gameService.undo(this.gameState.gameId).subscribe({
      next: (state) => (this.gameState = state),
      error: (err) => this.handleError(err)
    });
  }

  onResetGame(): void {
    if (!this.gameState) return;
    this.errorMessage = null;
    this.gameService.resetGame(this.gameState.gameId).subscribe({
      next: (state) => (this.gameState = state),
      error: (err) => this.handleError(err)
    });
  }

  onResetScoreboard(): void {
    if (!this.gameState) return;
    this.gameService.resetScoreboard().subscribe({
      next: (scoreboard) => {
        if (this.gameState) {
          this.gameState = { ...this.gameState, scoreboard };
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  private startNewGame(mode: GameMode): void {
    this.loading = true;
    this.errorMessage = null;
    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.gameState = state;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.handleError(err);
      }
    });
  }

  private handleError(err: HttpErrorResponse): void {
    this.errorMessage = err.error?.message ?? 'Something went wrong. Please try again.';
  }
}
