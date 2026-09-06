using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameLogic
{
    /// <summary>
    /// All 8 winning lines (3 rows, 3 columns, 2 diagonals) as cell indices.
    /// </summary>
    IReadOnlyList<int[]> WinningLines { get; }

    /// <summary>
    /// Checks whether the given player has completed a winning line on the board.
    /// Returns the winning cell indices, or an empty list if there is no win.
    /// </summary>
    List<int> CheckWin(Player?[] board, Player player);

    /// <summary>True when all 9 cells are filled.</summary>
    bool IsBoardFull(Player?[] board);

    /// <summary>
    /// Picks the computer's move (as O) following the required priority:
    /// 1) win if possible, 2) block X's win, 3) center, 4) corner, 5) any cell.
    /// Returns -1 if the board has no empty cell.
    /// </summary>
    int SelectComputerMove(Player?[] board);
}
