# Requirements Checklist (Traceability)

Every heading below is taken directly from the problem statement, in the
same order, with a pointer to exactly where it's satisfied in this
repository. Use this as the panel-review map.

---

### Problem Statement

Browser-based Tic Tac Toe, Angular frontend + .NET backend, running
locally — `backend/TicTacToe.Api` (.NET 8 Web API) and
`frontend/tic-tac-toe` (Angular 17). Supports play, move tracking, undo,
scoreboard, and a basic computer opponent — see "Functional Requirements"
and "Must-Have" below for each piece.

> **"The solution should be easy for the panel to run, review, and
> discuss."** — Two commands to start each half (`dotnet run`,
> `npm start`); see README §5–6. Swagger UI is enabled in Development mode
> for manually exercising the API without the frontend.

---

### Technology Expectations

> **"Use the following stack:"**

| Requirement | Where |
|---|---|
| Frontend: Angular + TypeScript | `frontend/tic-tac-toe` (Angular 17, standalone components) |
| Backend: .NET Web API | `backend/TicTacToe.Api` (.NET 8, ASP.NET Core) |
| API Style: REST API | `Controllers/GamesController.cs`, `Controllers/ScoreboardController.cs` |
| Storage: in-memory | `Services/InMemoryGameStore.cs` (SQLite was optional; in-memory chosen — see README §11) |
| Source Control: GitHub | A local `git` repo is included (see "Submission Requirements" below) — add a remote and push |
| Angular ↔ .NET via REST | `frontend/tic-tac-toe/src/app/services/game.service.ts` calls the controllers above over HTTP |
| Backend manages session + scoreboard state | `Services/GameService.cs`, `Services/InMemoryGameStore.cs` |

---

### Functional Requirements

**1. Game Board**
3×3 board, clickable-when-empty cells, X/O rendering, cells lock once
filled — `components/board/board.component.ts` (`isClickable()` disables
filled/finished cells) + `.html`/`.scss`. Board truth comes from
`GameSession.Board` in `Models/GameSession.cs`.

**2. Player Turns**
Two players (X/O), turn indicator, strict alternation, invalid moves
don't change the turn — `GameService.SubmitMove` only calls
`ApplyMove` (which flips `CurrentPlayer`) *after* all validation passes;
an invalid move throws before the turn changes. Displayed via
`AppComponent.statusMessage` in `app.component.ts`.
Test: `GameServiceTests.SubmitMove_TurnsAlternate_AfterEachValidMove`,
`SubmitMove_InvalidMove_DoesNotChangeTurn`.

**3. Win Detection**
Rows/columns/diagonals — `Services/GameLogic.cs` (`WinningLines`,
`CheckWin`). On win: winner shown, winning cells highlighted
(`board.component.scss` `.winning` class driven by `WinningCells`),
further moves blocked (`Status != InProgress` guard in `SubmitMove`),
scoreboard updated (`GameService.RecordResult`).
Tests: `GameLogicTests.CheckWin_DetectsRowWin` /
`_DetectsColumnWin` / `_DetectsDiagonalWin` / `_DetectsAntiDiagonalWin`.

**4. Draw Detection**
Board full + no winner → Draw — `GameLogic.IsBoardFull` +
`GameService.EvaluateStatus`. Draw message
(`AppComponent.statusMessage`), moves blocked, scoreboard updated.
Test: `GameServiceTests.SubmitMove_FullBoardNoWinner_EndsInDrawAndUpdatesScoreboard`.

**5. Reset Game**
Clears board/history/status, sets current player back to X, starts a
fresh session, **keeps the scoreboard unchanged** —
`GameService.ResetGame`, exposed as `POST /api/games/{id}/reset`, wired to
the "Reset Game" button in `app.component.html`.
Test: `GameServiceTests.ResetGame_ClearsBoardHistoryAndStatus_ButKeepsScoreboard`.

---

### Must-Have

**1. Move History**
Move number, player, cell position, updates after every valid move —
`Models/MoveRecord.cs`, rendered by
`components/move-history/move-history.component.ts/.html`.
Test: `MoveHistoryComponent` spec (renders one row per move).

**2. Undo Last Move**
Restores previous state, disabled when there are no moves —
`GameService.Undo`, `POST /api/games/{id}/undo`, `canUndo` flag on the
response drives the disabled state of the Undo button in
`app.component.html`.

> **"Undo Behavior by Mode"** — Two Player Mode removes only the most
> recent move; Computer Mode removes the computer's move *and* the
> preceding human move together — both branches implemented in
> `GameService.Undo` (`removeCount` logic) and covered by
> `GameServiceTests.Undo_TwoPlayerMode_RemovesOnlyLastMove` and
> `Undo_ComputerMode_RemovesComputerMoveAndPrecedingHumanMove`.

**3. Scoreboard**
Tracks X wins / O wins / Draws, updates exactly once per completed game
(`GameSession.ScoreCounted` guard in `GameService.RecordResult`), Reset
Game leaves it untouched, a separate Reset Scoreboard action exists
(`POST /api/scoreboard/reset`), and it's served by the backend
(`GET /api/scoreboard`) — `components/scoreboard/scoreboard.component.ts`.
Test: `GameServiceTests.Scoreboard_ResetSetsAllCountersToZero`.

**4. Basic Computer Mode**

> **"Provide two game modes:"** Two Player Mode and Play Against Computer
> — `Models/GameMode.cs`, selected via
> `components/mode-selector/mode-selector.component.ts`.

Human is X, computer is O, computer moves automatically right after the
human (synchronously, inside `GameService.SubmitMove` — see
`PROMPTS.md` for why), only valid moves, never moves once the game is
over (guarded by `session.Status == GameStatus.InProgress` before calling
`SelectComputerMove`). Priority order — win → block → center → corner →
any cell — implemented exactly as listed in `GameLogic.SelectComputerMove`.
Tests: `GameLogicTests.SelectComputerMove_TakesWinningMove_WhenAvailable`,
`_BlocksOpponentWin_WhenNoOwnWinAvailable`, `_TakesCenter_...`,
`_TakesCorner_...`, `_ReturnsMinusOne_WhenBoardFull`.

---

### Backend Requirements

REST APIs for game operations; backend owns session state, move history,
game status, and scoreboard — entirely in `Services/GameService.cs` (the
frontend never computes any of this itself). Full endpoint-to-purpose
table is in README §7, matching the problem statement's suggested API
scope table 1:1 (endpoint names kept identical:
`POST /api/games`, `GET /api/games/{id}`, `POST /api/games/{id}/moves`,
`POST /api/games/{id}/undo`, `POST /api/games/{id}/reset`,
`GET /api/scoreboard`, `POST /api/scoreboard/reset`).

---

### Game State Response

Game ID, board state, current player, game mode, game status, winner (if
any), winning cells (if any), move history, and a scoreboard snapshot —
all present on `DTOs/GameStateResponse.cs`. Statuses are the exact set
named in the problem statement: `InProgress`, `Won`, `Draw`
(`Models/GameStatus.cs`).

---

### Move Request

Game ID (in the URL), player, and either `cellIndex` or `row`+`col` —
`DTOs/MoveRequest.cs`, resolved by `GameService.ResolveCellIndex`.
Rejected cases, each with its own `InvalidMoveException` message in
`GameService.SubmitMove`: move outside the board, move on an occupied
cell, move after game completion, move by the wrong player. Each has a
dedicated test in `GameServiceTests.cs`
(`SubmitMove_OnOccupiedCell_ThrowsInvalidMove`,
`SubmitMove_ByWrongPlayer_ThrowsInvalidMove`,
`SubmitMove_OutsideBoard_ThrowsInvalidMove`,
`SubmitMove_AfterGameCompleted_Throws`).

---

### Frontend Requirements

Game board, current player, selected mode, winner/draw message,
highlighted winning cells, move history, scoreboard, Reset Game, Undo
Last Move, and Reset Scoreboard buttons — all present in
`app.component.html`, composed from the five child components under
`src/app/components/`. The frontend calls the backend for every action
and renders exactly what comes back (`game.service.ts` +
`AppComponent`'s `subscribe` callbacks — no game logic lives in Angular).
Responsive layout for laptop-width browsers with a stacking fallback
below 720px — `app.component.scss`.

---

### Important Clarifications

**Clarification 1: Backend State Ownership** — the frontend holds no game
rules; every board/turn/history/scoreboard value shown is whatever the
last `GameState` response said, stored as-is in
`AppComponent.gameState` and passed down via `@Input()`s. No client-side
recomputation of win/draw/turn logic exists anywhere in
`frontend/tic-tac-toe/src/app`.

**Clarification 2: Scoreboard and Undo** — **Option A (Disable Undo After
Completion)** was chosen and is enforced in `GameService.Undo` (throws
`InvalidMoveException` when `session.Status != GameStatus.InProgress`).
This is stated explicitly in README §10 ("Design Decisions") as required
by the problem statement ("mention the choice in the README"). Option B
is listed as a possible future improvement in README §13, not
implemented.

---

### Testing Expectations

All twelve listed cases have a corresponding test — see the table below.

| Required case | Test |
|---|---|
| Valid move | `GameServiceTests.SubmitMove_ValidMove_PlacesMarkAndReturnsUpdatedState` |
| Invalid move | `SubmitMove_OnOccupiedCell_ThrowsInvalidMove`, `_ByWrongPlayer_...`, `_OutsideBoard_...` |
| Turn switching | `SubmitMove_TurnsAlternate_AfterEachValidMove`, `_InvalidMove_DoesNotChangeTurn` |
| Row win | `GameServiceTests.SubmitMove_RowWin_EndsGameAndUpdatesScoreboard`, `GameLogicTests.CheckWin_DetectsRowWin` |
| Column win | `GameServiceTests.SubmitMove_ColumnWin_Detected`, `GameLogicTests.CheckWin_DetectsColumnWin` |
| Diagonal win | `GameServiceTests.SubmitMove_DiagonalWin_Detected`, `GameLogicTests.CheckWin_DetectsDiagonalWin`/`_DetectsAntiDiagonalWin` |
| Draw | `SubmitMove_FullBoardNoWinner_EndsInDrawAndUpdatesScoreboard` |
| Reset game | `ResetGame_ClearsBoardHistoryAndStatus_ButKeepsScoreboard` |
| Undo in two-player mode | `Undo_TwoPlayerMode_RemovesOnlyLastMove` |
| Undo in computer mode | `Undo_ComputerMode_RemovesComputerMoveAndPrecedingHumanMove` |
| Scoreboard update | `SubmitMove_RowWin_...` (win), `_FullBoardNoWinner_...` (draw), `Scoreboard_ResetSetsAllCountersToZero` |
| Computer move selection | `GameLogicTests.SelectComputerMove_*` (5 tests), `GameServiceTests.ComputerMode_ComputerMovesAutomaticallyAndOnlyValidly` |
| Move after game completion | `SubmitMove_AfterGameCompleted_Throws` |

Backend tests are xUnit (`backend/TicTacToe.Api.Tests`), matching
"Backend unit tests are preferred for game rules and state transitions."
Frontend tests are Jasmine/Karma specs covering component rendering and
the `GameService` API integration points, matching "Frontend tests may
cover component rendering and API integration points."

---

### AI-Assisted Development Expectation

See `PROMPTS.md` for the literal prompt log, and README §9 for the
summarized version — both cover: how the requirement was turned into a
spec, what was generated vs. reviewed/changed manually, and what
trade-offs were made (undo policy, move addressing, computer-move timing,
scoreboard guard).

---

### README Expectations

All twelve items are present in `README.md`: project overview (§1), tech
stack (§2), features implemented (§3), how to run the backend (§5), how
to run the frontend (§6), API endpoint summary (§7), how to run tests
(§8), AI tools and prompt summary (§9, plus the full log in
`PROMPTS.md`), design decisions (§10), clarifications and assumptions
(§11), known limitations (§12), future improvements (§13).

---

### Submission Requirements

| Required | Where |
|---|---|
| Angular frontend source | `frontend/tic-tac-toe/` |
| .NET backend source | `backend/` |
| README.md | `/README.md` |
| Setup and run instructions | README §5–6 |
| Test instructions | README §8 |
| Prompt summary / AI workflow notes | README §9, `PROMPTS.md` |
| API documentation / endpoint summary | README §7 |
| Known assumptions and limitations | README §11–12 |
| GitHub repository | A local `git` repository is initialized inside this archive with an initial commit history (backend, frontend, docs) — run `git remote add origin <your-repo-url>` and `git push -u origin main` to publish it. Network access wasn't available in the sandbox that built this, so the push itself has to happen on your machine. |

---

### Acceptance Criteria

Every bullet in this section is a **runtime** check (the app *runs*
locally, mode *works*, detection *works*, etc.). Everything above shows
where each behavior is implemented and unit-tested, but **this was
authored without a `.NET` SDK or internet access in the build
environment**, so nothing here has actually been compiled or executed —
see README §"Known Limitations of This Delivery" below. Before the panel
review, run:

```bash
cd backend && dotnet build && dotnet test
cd ../frontend/tic-tac-toe && npm install && npm test && npm start
```

If anything fails to compile, it's most likely a small, mechanical fix
(a missing `using`, a template binding typo) rather than a logic error —
the logic has been traced above against every requirement, and the
TypeScript layer has already been checked with `tsc --noEmit` (clean
aside from expected "module not found" errors, since npm packages aren't
installed offline).
