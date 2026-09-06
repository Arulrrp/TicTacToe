using FluentAssertions;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    private static GameService NewService() => new(new InMemoryGameStore(), new GameLogic());

    private static MoveRequest Move(Player player, int cellIndex) =>
        new() { Player = player, CellIndex = cellIndex };

    [Fact]
    public void SubmitMove_ValidMove_PlacesMarkAndReturnsUpdatedState()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.SubmitMove(game.GameId, Move(Player.X, 0));

        result.Board[0].Should().Be("X");
        result.Status.Should().Be(GameStatus.InProgress);
        result.MoveHistory.Should().HaveCount(1);
    }

    [Fact]
    public void SubmitMove_OnOccupiedCell_ThrowsInvalidMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));

        Action act = () => service.SubmitMove(game.GameId, Move(Player.O, 0));

        act.Should().Throw<InvalidMoveException>();
    }

    [Fact]
    public void SubmitMove_ByWrongPlayer_ThrowsInvalidMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X goes first; O attempting to move now is out of turn.
        Action act = () => service.SubmitMove(game.GameId, Move(Player.O, 0));

        act.Should().Throw<InvalidMoveException>();
    }

    [Fact]
    public void SubmitMove_OutsideBoard_ThrowsInvalidMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Action act = () => service.SubmitMove(game.GameId, Move(Player.X, 9));

        act.Should().Throw<InvalidMoveException>();
    }

    [Fact]
    public void SubmitMove_TurnsAlternate_AfterEachValidMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var afterX = service.SubmitMove(game.GameId, Move(Player.X, 0));
        afterX.CurrentPlayer.Should().Be(Player.O);

        var afterO = service.SubmitMove(game.GameId, Move(Player.O, 1));
        afterO.CurrentPlayer.Should().Be(Player.X);
    }

    [Fact]
    public void SubmitMove_InvalidMove_DoesNotChangeTurn()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));

        // O attempts an occupied cell; should fail and not advance the turn.
        Action act = () => service.SubmitMove(game.GameId, Move(Player.O, 0));
        act.Should().Throw<InvalidMoveException>();

        var state = service.GetGame(game.GameId);
        state.CurrentPlayer.Should().Be(Player.O);
    }

    [Fact]
    public void SubmitMove_RowWin_EndsGameAndUpdatesScoreboard()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,1,2 (row 1) | O: 3,4
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 3));
        service.SubmitMove(game.GameId, Move(Player.X, 1));
        service.SubmitMove(game.GameId, Move(Player.O, 4));
        var result = service.SubmitMove(game.GameId, Move(Player.X, 2));

        result.Status.Should().Be(GameStatus.Won);
        result.Winner.Should().Be(Player.X);
        result.WinningCells.Should().BeEquivalentTo(new[] { 0, 1, 2 });
        result.Scoreboard.XWins.Should().Be(1);
    }

    [Fact]
    public void SubmitMove_ColumnWin_Detected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,3,6 (col 1) | O: 1,2
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 1));
        service.SubmitMove(game.GameId, Move(Player.X, 3));
        service.SubmitMove(game.GameId, Move(Player.O, 2));
        var result = service.SubmitMove(game.GameId, Move(Player.X, 6));

        result.Status.Should().Be(GameStatus.Won);
        result.WinningCells.Should().BeEquivalentTo(new[] { 0, 3, 6 });
    }

    [Fact]
    public void SubmitMove_DiagonalWin_Detected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,4,8 (diagonal) | O: 1,2
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 1));
        service.SubmitMove(game.GameId, Move(Player.X, 4));
        service.SubmitMove(game.GameId, Move(Player.O, 2));
        var result = service.SubmitMove(game.GameId, Move(Player.X, 8));

        result.Status.Should().Be(GameStatus.Won);
        result.WinningCells.Should().BeEquivalentTo(new[] { 0, 4, 8 });
    }

    [Fact]
    public void SubmitMove_FullBoardNoWinner_EndsInDrawAndUpdatesScoreboard()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X | O | X
        // X | O | O
        // O | X | X
        var sequence = new[]
        {
            (Player.X, 0), (Player.O, 1), (Player.X, 2),
            (Player.O, 4), (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6), (Player.X, 8)
        };

        GameStateResponse? last = null;
        foreach (var (player, cell) in sequence)
        {
            last = service.SubmitMove(game.GameId, Move(player, cell));
        }

        last!.Status.Should().Be(GameStatus.Draw);
        last.Winner.Should().BeNull();
        last.Scoreboard.Draws.Should().Be(1);
    }

    [Fact]
    public void SubmitMove_AfterGameCompleted_Throws()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 3));
        service.SubmitMove(game.GameId, Move(Player.X, 1));
        service.SubmitMove(game.GameId, Move(Player.O, 4));
        service.SubmitMove(game.GameId, Move(Player.X, 2)); // X wins

        Action act = () => service.SubmitMove(game.GameId, Move(Player.O, 5));

        act.Should().Throw<InvalidMoveException>();
    }

    [Fact]
    public void ResetGame_ClearsBoardHistoryAndStatus_ButKeepsScoreboard()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 3));
        service.SubmitMove(game.GameId, Move(Player.X, 1));
        service.SubmitMove(game.GameId, Move(Player.O, 4));
        service.SubmitMove(game.GameId, Move(Player.X, 2)); // X wins, scoreboard += 1

        var reset = service.ResetGame(game.GameId);

        reset.Board.Should().OnlyContain(cell => cell == null);
        reset.MoveHistory.Should().BeEmpty();
        reset.Status.Should().Be(GameStatus.InProgress);
        reset.CurrentPlayer.Should().Be(Player.X);
        reset.Scoreboard.XWins.Should().Be(1); // unchanged by reset
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 4));

        var afterUndo = service.Undo(game.GameId);

        afterUndo.MoveHistory.Should().HaveCount(1);
        afterUndo.Board[4].Should().BeNull();
        afterUndo.Board[0].Should().Be("X");
        afterUndo.CurrentPlayer.Should().Be(Player.O);
    }

    [Fact]
    public void Undo_ComputerMode_RemovesComputerMoveAndPrecedingHumanMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.VsComputer);

        // Human plays X; the service immediately plays O's computer move too.
        var afterHumanMove = service.SubmitMove(game.GameId, Move(Player.X, 0));
        afterHumanMove.MoveHistory.Should().HaveCount(2); // X then computer O

        var afterUndo = service.Undo(game.GameId);

        afterUndo.MoveHistory.Should().BeEmpty();
        afterUndo.Board.Should().OnlyContain(cell => cell == null);
        afterUndo.CurrentPlayer.Should().Be(Player.X);
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Action act = () => service.Undo(game.GameId);

        act.Should().Throw<InvalidMoveException>();
    }

    [Fact]
    public void ComputerMode_ComputerMovesAutomaticallyAndOnlyValidly()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.VsComputer);

        var result = service.SubmitMove(game.GameId, Move(Player.X, 0));

        // Exactly one computer move should have been made, into an empty cell.
        result.MoveHistory.Should().HaveCount(2);
        result.MoveHistory[1].Player.Should().Be(Player.O);
        result.Board.Count(c => c != null).Should().Be(2);
    }

    [Fact]
    public void Scoreboard_ResetSetsAllCountersToZero()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.SubmitMove(game.GameId, Move(Player.X, 0));
        service.SubmitMove(game.GameId, Move(Player.O, 3));
        service.SubmitMove(game.GameId, Move(Player.X, 1));
        service.SubmitMove(game.GameId, Move(Player.O, 4));
        service.SubmitMove(game.GameId, Move(Player.X, 2)); // X wins

        service.GetScoreboard().XWins.Should().Be(1);

        var reset = service.ResetScoreboard();

        reset.XWins.Should().Be(0);
        reset.OWins.Should().Be(0);
        reset.Draws.Should().Be(0);
    }
}
