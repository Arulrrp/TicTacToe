namespace TicTacToe.Api.Models;

/// <summary>
/// Server-owned state for a single game. The backend is the single
/// source of truth: board, current player, status, winner and move
/// history are all derived/maintained here, never trusted from the client.
/// </summary>
public class GameSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public GameMode Mode { get; set; } = GameMode.TwoPlayer;

    /// <summary>9 cells, index 0-8, row-major. Null = empty.</summary>
    public Player?[] Board { get; set; } = new Player?[9];

    public Player CurrentPlayer { get; set; } = Player.X;

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public Player? Winner { get; set; }

    public List<int> WinningCells { get; set; } = new();

    public List<MoveRecord> MoveHistory { get; set; } = new();

    /// <summary>
    /// Guards against double-counting the scoreboard if a completed
    /// game were ever re-evaluated. Set exactly once, the first time
    /// a game transitions into Won or Draw.
    /// </summary>
    public bool ScoreCounted { get; set; }
}
