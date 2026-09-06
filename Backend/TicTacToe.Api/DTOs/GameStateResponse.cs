using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class MoveHistoryItemDto
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int CellIndex { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}

public class GameStateResponse
{
    public string GameId { get; set; } = string.Empty;

    /// <summary>9 cells, index 0-8. Each is "X", "O" or null for empty.</summary>
    public string?[] Board { get; set; } = new string?[9];

    public Player CurrentPlayer { get; set; }

    public GameMode Mode { get; set; }

    public GameStatus Status { get; set; }

    public Player? Winner { get; set; }

    public List<int> WinningCells { get; set; } = new();

    public List<MoveHistoryItemDto> MoveHistory { get; set; } = new();

    public bool CanUndo { get; set; }

    public ScoreboardResponse Scoreboard { get; set; } = new();
}
