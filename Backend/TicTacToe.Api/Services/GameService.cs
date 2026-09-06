using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Owns all game rules orchestration. The backend is the single source
/// of truth for board state, turn order, win/draw detection, move
/// history and the scoreboard - the frontend only ever renders what
/// this service returns.
///
/// Undo policy (see Clarification 2 in the problem statement): this
/// implementation uses Option A - "Disable Undo After Completion".
/// Once a game is Won or Draw, Undo is rejected and the scoreboard
/// entry for that game is final. This keeps scoreboard bookkeeping
/// simple and unambiguous (see README "Design decisions").
/// </summary>
public class GameService : IGameService
{
    private readonly IGameStore _store;
    private readonly IGameLogic _logic;

    public GameService(IGameStore store, IGameLogic logic)
    {
        _store = store;
        _logic = logic;
    }

    public GameStateResponse CreateGame(GameMode mode)
    {
        var session = new GameSession { Mode = mode };
        _store.Create(session);
        return ToResponse(session);
    }

    public GameStateResponse GetGame(string gameId)
    {
        var session = _store.Get(gameId) ?? throw new GameNotFoundException(gameId);
        return ToResponse(session);
    }

    public GameStateResponse SubmitMove(string gameId, MoveRequest request)
    {
        var session = _store.Get(gameId) ?? throw new GameNotFoundException(gameId);

        if (session.Status != GameStatus.InProgress)
        {
            throw new InvalidMoveException("This game has already finished. Reset to start a new one.");
        }

        var cellIndex = ResolveCellIndex(request);

        if (cellIndex < 0 || cellIndex > 8)
        {
            throw new InvalidMoveException("Move is outside the board.");
        }

        if (request.Player != session.CurrentPlayer)
        {
            throw new InvalidMoveException($"It is not {request.Player}'s turn.");
        }

        if (session.Board[cellIndex].HasValue)
        {
            throw new InvalidMoveException("That cell is already occupied.");
        }

        ApplyMove(session, request.Player, cellIndex);

        // Automatically play the computer's turn (O) right after the
        // human's move, as required for Computer Mode.
        if (session.Mode == GameMode.VsComputer
            && session.Status == GameStatus.InProgress
            && session.CurrentPlayer == Player.O)
        {
            var computerCell = _logic.SelectComputerMove(session.Board);
            if (computerCell >= 0)
            {
                ApplyMove(session, Player.O, computerCell);
            }
        }

        _store.Save(session);
        return ToResponse(session);
    }

    public GameStateResponse Undo(string gameId)
    {
        var session = _store.Get(gameId) ?? throw new GameNotFoundException(gameId);

        if (session.Status != GameStatus.InProgress)
        {
            throw new InvalidMoveException("Undo is disabled once a game has finished.");
        }

        if (session.MoveHistory.Count == 0)
        {
            throw new InvalidMoveException("There are no moves to undo.");
        }

        var removeCount = 1;
        if (session.Mode == GameMode.VsComputer
            && session.MoveHistory[^1].Player == Player.O
            && session.MoveHistory.Count >= 2)
        {
            // Undo the computer's move together with the human move before it.
            removeCount = 2;
        }

        session.MoveHistory.RemoveRange(
            session.MoveHistory.Count - removeCount,
            removeCount);

        RebuildFromHistory(session);
        _store.Save(session);
        return ToResponse(session);
    }

    public GameStateResponse ResetGame(string gameId)
    {
        var session = _store.Get(gameId) ?? throw new GameNotFoundException(gameId);

        session.Board = new Player?[9];
        session.CurrentPlayer = Player.X;
        session.Status = GameStatus.InProgress;
        session.Winner = null;
        session.WinningCells = new List<int>();
        session.MoveHistory = new List<MoveRecord>();
        session.ScoreCounted = false;
        // Mode is preserved deliberately; the scoreboard is untouched.

        _store.Save(session);
        return ToResponse(session);
    }

    public ScoreboardResponse GetScoreboard()
    {
        var board = _store.GetScoreboard();
        return new ScoreboardResponse { XWins = board.XWins, OWins = board.OWins, Draws = board.Draws };
    }

    public ScoreboardResponse ResetScoreboard()
    {
        var fresh = new Scoreboard();
        _store.SaveScoreboard(fresh);
        return new ScoreboardResponse();
    }

    // ---- internal helpers -------------------------------------------------

    private static int ResolveCellIndex(MoveRequest request)
    {
        if (request.CellIndex.HasValue)
        {
            return request.CellIndex.Value;
        }

        if (request.Row.HasValue && request.Col.HasValue)
        {
            if (request.Row.Value is < 0 or > 2 || request.Col.Value is < 0 or > 2)
            {
                return -1;
            }
            return (request.Row.Value * 3) + request.Col.Value;
        }

        throw new InvalidMoveException("Either cellIndex or both row and col must be provided.");
    }

    /// <summary>
    /// Places a move on the board, records it, and recalculates win/draw
    /// status - updating the scoreboard exactly once if the game just
    /// finished - then advances the turn.
    /// </summary>
    private void ApplyMove(GameSession session, Player player, int cellIndex)
    {
        session.Board[cellIndex] = player;

        var moveNumber = session.MoveHistory.Count + 1;
        session.MoveHistory.Add(new MoveRecord
        {
            MoveNumber = moveNumber,
            Player = player,
            CellIndex = cellIndex,
            Row = cellIndex / 3,
            Col = cellIndex % 3
        });

        EvaluateStatus(session, player);

        if (session.Status == GameStatus.InProgress)
        {
            session.CurrentPlayer = player == Player.X ? Player.O : Player.X;
        }
    }

    /// <summary>
    /// Checks for a win by the player who just moved, or a draw, and
    /// updates the scoreboard exactly once when a game completes.
    /// </summary>
    private void EvaluateStatus(GameSession session, Player justMoved)
    {
        var winningCells = _logic.CheckWin(session.Board, justMoved);
        if (winningCells.Count > 0)
        {
            session.Status = GameStatus.Won;
            session.Winner = justMoved;
            session.WinningCells = winningCells;
            RecordResult(session, justMoved);
            return;
        }

        if (_logic.IsBoardFull(session.Board))
        {
            session.Status = GameStatus.Draw;
            session.Winner = null;
            session.WinningCells = new List<int>();
            RecordResult(session, null);
        }
    }

    private void RecordResult(GameSession session, Player? winner)
    {
        if (session.ScoreCounted) return;

        var board = _store.GetScoreboard();
        if (winner == Player.X) board.XWins++;
        else if (winner == Player.O) board.OWins++;
        else board.Draws++;

        _store.SaveScoreboard(board);
        session.ScoreCounted = true;
    }

    /// <summary>
    /// Recomputes board, current player and status from the (already
    /// truncated) move list. Used after Undo. Because a win can only ever
    /// occur on the final move of a completed game, and Undo is refused
    /// once a game is completed, any prefix left after Undo is guaranteed
    /// to be InProgress - this is still recalculated defensively rather
    /// than assumed.
    /// </summary>
    private void RebuildFromHistory(GameSession session)
    {
        var board = new Player?[9];
        foreach (var move in session.MoveHistory)
        {
            board[move.CellIndex] = move.Player;
        }
        session.Board = board;
        session.CurrentPlayer = session.MoveHistory.Count % 2 == 0 ? Player.X : Player.O;
        session.Status = GameStatus.InProgress;
        session.Winner = null;
        session.WinningCells = new List<int>();
        session.ScoreCounted = false;

        // Defensive re-check in case of an unexpected edge case.
        foreach (var player in new[] { Player.X, Player.O })
        {
            var win = _logic.CheckWin(board, player);
            if (win.Count > 0)
            {
                session.Status = GameStatus.Won;
                session.Winner = player;
                session.WinningCells = win;
                return;
            }
        }
        if (_logic.IsBoardFull(board))
        {
            session.Status = GameStatus.Draw;
        }
    }

    private GameStateResponse ToResponse(GameSession session)
    {
        var scoreboard = _store.GetScoreboard();
        return new GameStateResponse
        {
            GameId = session.Id,
            Board = session.Board.Select(cell => cell?.ToString()).ToArray(),
            CurrentPlayer = session.CurrentPlayer,
            Mode = session.Mode,
            Status = session.Status,
            Winner = session.Winner,
            WinningCells = session.WinningCells,
            MoveHistory = session.MoveHistory.Select(move => new MoveHistoryItemDto
            {
                MoveNumber = move.MoveNumber,
                Player = move.Player,
                CellIndex = move.CellIndex,
                Row = move.Row,
                Col = move.Col
            }).ToList(),
            CanUndo = session.Status == GameStatus.InProgress && session.MoveHistory.Count > 0,
            Scoreboard = new ScoreboardResponse
            {
                XWins = scoreboard.XWins,
                OWins = scoreboard.OWins,
                Draws = scoreboard.Draws
            }
        };
    }
}
