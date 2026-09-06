namespace TicTacToe.Api.Models;

/// <summary>
/// A single recorded move, used both to render move history and to
/// rebuild board state (e.g. after an Undo) by replaying moves in order.
/// </summary>
public class MoveRecord
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int CellIndex { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}
