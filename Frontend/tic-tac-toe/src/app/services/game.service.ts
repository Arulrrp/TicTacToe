import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GameMode, GameState, PlayerSymbol, Scoreboard } from '../models/game.model';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = `${environment.apiUrl}/games`;
  private readonly scoreboardUrl = `${environment.apiUrl}/scoreboard`;

  constructor(private http: HttpClient) {}

  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(this.baseUrl, { mode });
  }

  getGame(gameId: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/${gameId}`);
  }

  submitMove(gameId: string, player: PlayerSymbol, cellIndex: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${gameId}/moves`, {
      player,
      cellIndex
    });
  }

  undo(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${gameId}/undo`, {});
  }

  resetGame(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/${gameId}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(this.scoreboardUrl);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.scoreboardUrl}/reset`, {});
  }
}
