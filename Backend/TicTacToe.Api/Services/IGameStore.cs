using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameStore
{
    GameSession Create(GameSession session);
    GameSession? Get(string id);
    void Save(GameSession session);

    Scoreboard GetScoreboard();
    void SaveScoreboard(Scoreboard scoreboard);
}
