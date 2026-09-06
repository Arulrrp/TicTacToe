# Tic Tac Toe (Angular + .NET)

A browser-based Tic Tac Toe app with an Angular frontend and a .NET Web API
backend, built for the Round 2 take-home exercise. The backend owns all game
state (board, turns, win/draw detection, move history and the scoreboard);
the Angular app only renders what the API returns and calls REST endpoints
for every action.

---

## 1. Project Overview

Two players (or a player vs. a simple computer opponent) play Tic Tac Toe in
the browser. Every move is validated and applied on the server; the frontend
is a thin client that reflects server state. Supported features:

- 3x3 board, click-to-play, cells lock once filled
- Two Player mode and Play-Against-Computer mode
- Turn indicator, alternating turns, invalid moves rejected without changing
  the turn
- Win detection (rows, columns, diagonals) with winning-cell highlighting
- Draw detection
- Move history (move number, player, row/column)
- Undo Last Move (mode-aware — see "Design Decisions")
- Session scoreboard (X wins / O wins / draws) with an independent
  Reset Scoreboard action
- Reset Game (clears the board/history/status, keeps the scoreboard)

## 2. Tech Stack

| Layer     | Technology                                   |
|-----------|-----------------------------------------------|
| Frontend  | Angular 17 (standalone components), TypeScript, RxJS |
| Backend   | .NET 8, ASP.NET Core Web API                   |
| API style | REST (JSON)                                    |
| Storage   | In-memory (`ConcurrentDictionary`, singleton service) |
| Testing   | xUnit + FluentAssertions (backend), Jasmine/Karma (frontend) |

## 3. Features Implemented

All functional requirements from the problem statement: game board, player
turns, win detection, draw detection, reset game, move history, undo (mode
aware), scoreboard, and basic computer opponent (win → block → center →
corner → any cell priority). See section 1 above for the checklist.

## 4. Project Structure

```
.
├── backend/
│   ├── TicTacToe.Api/            # ASP.NET Core Web API
│   │   ├── Controllers/          # GamesController, ScoreboardController
│   │   ├── Services/             # GameService, GameLogic, InMemoryGameStore
│   │   ├── Models/                # GameSession, MoveRecord, enums
│   │   ├── DTOs/                  # Request/response contracts
│   │   └── Program.cs
│   ├── TicTacToe.Api.Tests/      # xUnit tests
│   └── TicTacToe.sln
└── frontend/
    └── tic-tac-toe/              # Angular application
        └── src/app/
            ├── components/        # board, scoreboard, move-history, mode-selector
            ├── services/          # game.service.ts (all HTTP calls)
            └── models/             # game.model.ts
```

## 5. How to Run the Backend Locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd backend/TicTacToe.Api
dotnet restore
dotnet run
```

The API listens on **http://localhost:5183** (configured in
`Properties/launchSettings.json`) and Swagger UI is available at
`http://localhost:5183/swagger` in Development mode. CORS is pre-configured
to allow the Angular dev server at `http://localhost:4200`.

## 6. How to Run the Frontend Locally

Requires [Node.js 18+](https://nodejs.org/) and npm.

```bash
cd frontend/tic-tac-toe
npm install
npm start
```

This runs `ng serve` on **http://localhost:4200**. The app is pre-configured
(`src/environments/environment.ts`) to call the backend at
`http://localhost:5183/api` — start the backend first (or in parallel; the
app will show a connection error until it's reachable).

## 7. API Endpoint Summary

| Method | Endpoint                     | Purpose                                 |
|--------|-------------------------------|------------------------------------------|
| POST   | `/api/games`                  | Create a new game session (`{ mode: "TwoPlayer" \| "VsComputer" }`) |
| GET    | `/api/games/{id}`             | Get current game state                   |
| POST   | `/api/games/{id}/moves`       | Submit a move (`{ player, cellIndex }` or `{ player, row, col }`) |
| POST   | `/api/games/{id}/undo`        | Undo the last move (or move pair)         |
| POST   | `/api/games/{id}/reset`       | Reset the current game; scoreboard unaffected |
| GET    | `/api/scoreboard`             | Get the scoreboard                        |
| POST   | `/api/scoreboard/reset`       | Reset the scoreboard to zero              |

**Game state response** includes: `gameId`, `board` (9-element array of
`"X" | "O" | null`), `currentPlayer`, `mode`, `status`
(`InProgress | Won | Draw`), `winner`, `winningCells`, `moveHistory`,
`canUndo`, and an embedded `scoreboard` snapshot.

The backend rejects invalid moves with `400 Bad Request` and a JSON
`{ message }` body: moves outside the board, on an occupied cell, after the
game has completed, or by the wrong player. Unknown game ids return
`404 Not Found`.

## 8. How to Run Tests

**Backend (xUnit):**

```bash
cd backend
dotnet test
```

Covers: valid/invalid moves, turn switching (including that invalid moves
don't change the turn), row/column/diagonal win detection, draw detection,
reset game, undo in both Two Player and Computer mode, scoreboard updates,
computer move selection priority, and move-after-completion rejection.

**Frontend (Jasmine/Karma):**

```bash
cd frontend/tic-tac-toe
npm test
```

Covers the `GameService` HTTP calls, the `BoardComponent` (rendering,
click/disabled behavior, winning-cell highlighting), `ScoreboardComponent`,
`MoveHistoryComponent`, `ModeSelectorComponent`, and the root
`AppComponent`'s orchestration (create/move/undo/reset flows and error
handling).

## 9. AI-Assisted Development Notes

This solution was built with AI assistance (Claude). Summary of the workflow:

- **Specification:** the problem statement (functional requirements, API
  scope, undo semantics, computer-move priority, testing expectations) was
  turned directly into the backend contract (DTOs and controller routes)
  and the frontend component breakdown.
- **Prompt approach:** asked for the full stack — a .NET Web API backend
  owning game/session/scoreboard state, and an Angular frontend that only
  renders backend responses — with in-memory storage, REST endpoints
  matching the suggested API scope, and unit tests covering the listed
  scenarios.
- **What the AI generated:** the full backend (models, DTOs, game logic,
  service orchestration, controllers, DI/CORS wiring, xUnit tests) and the
  full frontend (standalone Angular components, the HTTP service layer,
  templates/styles, Karma specs) plus this README.
- **What was reviewed/decided manually:** the Undo policy (Option A vs. B),
  how a move is addressed (`cellIndex` vs. `row`/`col`, supporting both),
  where the computer's automatic move is triggered (synchronously within
  the same `/moves` call so the frontend never has to poll), and the
  decision to keep `GameSession.ScoreCounted` as an explicit guard against
  double-counting the scoreboard.
- **Assumptions/trade-offs made in the process are captured in sections 10
  and 11 below**, so they're easy to explain during review rather than
  buried in code comments alone.

## 10. Design Decisions

- **Undo policy — Option A (Disable Undo After Completion).** Once a game's
  status is `Won` or `Draw`, `POST /undo` returns `400`. This keeps the
  scoreboard simple and unambiguous: a completed game's result is final the
  moment it's recorded, with no need to "unwind" a scoreboard increment.
- **Undo implemented by replay, not by snapshotting.** Undo removes the last
  move (or, in Computer Mode, the last human+computer pair) from the stored
  move list, then rebuilds the board, current player, and status by
  replaying the remaining moves. This keeps a single source of truth (the
  move list) rather than maintaining a parallel undo stack.
- **The computer's reply is synchronous.** In Computer Mode, after a valid
  human move the backend immediately computes and applies the computer's
  move within the same request, so the frontend always receives a fully
  "settled" state and never has to poll for the computer's turn.
- **Move addressing supports both `cellIndex` and `row`/`col`** so the
  frontend could be built either way; the Angular client uses `cellIndex`.
- **Scoreboard is a separate backend-owned resource** from any single game,
  so Reset Game and Reset Scoreboard are independent actions, and a game's
  result is only ever counted once (`ScoreCounted` guard).

## 11. Clarifications and Assumptions

- Storage is in-memory only (as permitted); state resets when the backend
  process restarts.
- A single backend process is assumed (in-memory `ConcurrentDictionary`
  isn't shared across multiple instances/replicas).
- The frontend assumes the backend is reachable at
  `http://localhost:5183/api` in development (`src/environments/environment.ts`).
- Switching game mode (Two Player ↔ Computer) starts a brand-new game
  session rather than converting the in-progress one, since the two modes
  have different undo semantics.
- "Move by the wrong player" is enforced by comparing the submitted
  `player` to the session's `currentPlayer` — the frontend doesn't need to
  track this itself, but it must send the correct value.

## 12. Known Limitations

- No persistence: restarting the backend clears all games and the
  scoreboard.
- No authentication/multi-user isolation: the API is designed for a single
  local reviewer running one game at a time (matching "easy for the panel
  to run, review, and discuss").
- The computer opponent is intentionally simple (rule-based priority list,
  not a full minimax), per "Basic Computer Mode" in the requirements.

## 13. Future Improvements

- Optional SQLite persistence (already anticipated by the problem
  statement) for surviving backend restarts.
- Option B undo behavior (allow undo after completion with scoreboard
  correction) as a togglable mode.
- A minimax/unbeatable computer difficulty option.
- WebSocket/SignalR push for a true two-browser multiplayer experience.
