export type PlayerSymbol = 'X' | 'O';

export type GameMode = 'TwoPlayer' | 'VsComputer';

export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveHistoryItem {
  moveNumber: number;
  player: PlayerSymbol;
  cellIndex: number;
  row: number;
  col: number;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  board: (PlayerSymbol | null)[];
  currentPlayer: PlayerSymbol;
  mode: GameMode;
  status: GameStatus;
  winner: PlayerSymbol | null;
  winningCells: number[];
  moveHistory: MoveHistoryItem[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}

export interface ApiError {
  message: string;
}
