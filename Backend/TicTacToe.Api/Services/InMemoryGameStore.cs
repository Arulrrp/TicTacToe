using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Storage is in-memory as permitted by the problem statement. Registered
/// as a singleton so state survives across requests for the lifetime of
/// the process. A ConcurrentDictionary keeps it safe under Kestrel's
/// multi-threaded request handling.
/// </summary>
public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();
    private Scoreboard _scoreboard = new();
    private readonly object _scoreboardLock = new();

    public GameSession Create(GameSession session)
    {
        _games[session.Id] = session;
        return session;
    }

    public GameSession? Get(string id)
    {
        return _games.TryGetValue(id, out var session) ? session : null;
    }

    public void Save(GameSession session)
    {
        _games[session.Id] = session;
    }

    public Scoreboard GetScoreboard()
    {
        lock (_scoreboardLock)
        {
            // Return a copy so callers can't mutate internal state directly.
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void SaveScoreboard(Scoreboard scoreboard)
    {
        lock (_scoreboardLock)
        {
            _scoreboard = scoreboard;
        }
    }
}
