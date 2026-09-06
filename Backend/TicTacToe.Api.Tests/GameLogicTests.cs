using FluentAssertions;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameLogicTests
{
    private readonly GameLogic _logic = new();

    private static Player?[] EmptyBoard() => new Player?[9];

    [Fact]
    public void CheckWin_DetectsRowWin()
    {
        var board = EmptyBoard();
        board[0] = board[1] = board[2] = Player.X;

        var result = _logic.CheckWin(board, Player.X);

        result.Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    [Fact]
    public void CheckWin_DetectsColumnWin()
    {
        var board = EmptyBoard();
        board[0] = board[3] = board[6] = Player.O;

        var result = _logic.CheckWin(board, Player.O);

        result.Should().BeEquivalentTo(new[] { 0, 3, 6 });
    }

    [Fact]
    public void CheckWin_DetectsDiagonalWin()
    {
        var board = EmptyBoard();
        board[0] = board[4] = board[8] = Player.X;

        var result = _logic.CheckWin(board, Player.X);

        result.Should().BeEquivalentTo(new[] { 0, 4, 8 });
    }

    [Fact]
    public void CheckWin_DetectsAntiDiagonalWin()
    {
        var board = EmptyBoard();
        board[2] = board[4] = board[6] = Player.O;

        var result = _logic.CheckWin(board, Player.O);

        result.Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    [Fact]
    public void CheckWin_ReturnsEmpty_WhenNoWinExists()
    {
        var board = EmptyBoard();
        board[0] = Player.X;
        board[1] = Player.O;

        _logic.CheckWin(board, Player.X).Should().BeEmpty();
    }

    [Fact]
    public void IsBoardFull_TrueOnlyWhenEveryCellFilled()
    {
        var board = EmptyBoard();
        _logic.IsBoardFull(board).Should().BeFalse();

        for (var i = 0; i < 9; i++) board[i] = i % 2 == 0 ? Player.X : Player.O;

        _logic.IsBoardFull(board).Should().BeTrue();
    }

    [Fact]
    public void SelectComputerMove_TakesWinningMove_WhenAvailable()
    {
        // O has two in a row on the top row; should complete the win at index 2.
        var board = EmptyBoard();
        board[0] = Player.O;
        board[1] = Player.O;
        board[4] = Player.X;

        _logic.SelectComputerMove(board).Should().Be(2);
    }

    [Fact]
    public void SelectComputerMove_BlocksOpponentWin_WhenNoOwnWinAvailable()
    {
        // X threatens to win on the left column; O has no winning move itself.
        var board = EmptyBoard();
        board[0] = Player.X;
        board[3] = Player.X;
        board[8] = Player.O;

        _logic.SelectComputerMove(board).Should().Be(6);
    }

    [Fact]
    public void SelectComputerMove_TakesCenter_WhenNoImmediateWinOrBlock()
    {
        var board = EmptyBoard();
        board[0] = Player.X;

        _logic.SelectComputerMove(board).Should().Be(4);
    }

    [Fact]
    public void SelectComputerMove_TakesCorner_WhenCenterTaken()
    {
        var board = EmptyBoard();
        board[4] = Player.X;

        var move = _logic.SelectComputerMove(board);

        new[] { 0, 2, 6, 8 }.Should().Contain(move);
    }

    [Fact]
    public void SelectComputerMove_ReturnsMinusOne_WhenBoardFull()
    {
        var board = EmptyBoard();
        for (var i = 0; i < 9; i++) board[i] = i % 2 == 0 ? Player.X : Player.O;

        _logic.SelectComputerMove(board).Should().Be(-1);
    }
}
