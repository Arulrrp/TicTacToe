using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Pure, stateless Tic Tac Toe rules. Kept free of any session/storage
/// concerns so it is trivial to unit test in isolation.
/// </summary>
public class GameLogic : IGameLogic
{
    private static readonly int[] Corners = { 0, 2, 6, 8 };
    private const int Center = 4;

    public IReadOnlyList<int[]> WinningLines { get; } = new List<int[]>
    {
        new[] { 0, 1, 2 }, // row 1
        new[] { 3, 4, 5 }, // row 2
        new[] { 6, 7, 8 }, // row 3
        new[] { 0, 3, 6 }, // col 1
        new[] { 1, 4, 7 }, // col 2
        new[] { 2, 5, 8 }, // col 3
        new[] { 0, 4, 8 }, // diagonal
        new[] { 2, 4, 6 }, // anti-diagonal
    };

    public List<int> CheckWin(Player?[] board, Player player)
    {
        foreach (var line in WinningLines)
        {
            if (line.All(cell => board[cell] == player))
            {
                return line.ToList();
            }
        }
        return new List<int>();
    }

    public bool IsBoardFull(Player?[] board) => board.All(cell => cell.HasValue);

    public int SelectComputerMove(Player?[] board)
    {
        var empties = EmptyCells(board);
        if (empties.Count == 0) return -1;

        // 1. Win if possible.
        var winMove = FindDecidingMove(board, Player.O);
        if (winMove != -1) return winMove;

        // 2. Block X's win.
        var blockMove = FindDecidingMove(board, Player.X);
        if (blockMove != -1) return blockMove;

        // 3. Center.
        if (board[Center] is null) return Center;

        // 4. A corner.
        foreach (var corner in Corners)
        {
            if (board[corner] is null) return corner;
        }

        // 5. Any available cell.
        return empties[0];
    }

    /// <summary>
    /// Returns the index of a cell that would complete a winning line for
    /// <paramref name="player"/> if played right now, or -1 if none exists.
    /// </summary>
    private int FindDecidingMove(Player?[] board, Player player)
    {
        foreach (var index in EmptyCells(board))
        {
            var trial = (Player?[])board.Clone();
            trial[index] = player;
            if (CheckWin(trial, player).Count > 0)
            {
                return index;
            }
        }
        return -1;
    }

    private static List<int> EmptyCells(Player?[] board)
    {
        var result = new List<int>();
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] is null) result.Add(i);
        }
        return result;
    }
}
