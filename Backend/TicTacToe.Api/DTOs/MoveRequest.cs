using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

/// <summary>
/// The frontend may address a cell either with (row, col) or a single
/// 0-8 cellIndex. At least one addressing scheme must be supplied.
/// </summary>
public class MoveRequest
{
    public Player Player { get; set; }

    public int? Row { get; set; }

    public int? Col { get; set; }

    public int? CellIndex { get; set; }
}
