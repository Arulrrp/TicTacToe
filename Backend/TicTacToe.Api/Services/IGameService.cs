using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameStateResponse CreateGame(GameMode mode);
    GameStateResponse GetGame(string gameId);
    GameStateResponse SubmitMove(string gameId, MoveRequest request);
    GameStateResponse Undo(string gameId);
    GameStateResponse ResetGame(string gameId);

    ScoreboardResponse GetScoreboard();
    ScoreboardResponse ResetScoreboard();
}
