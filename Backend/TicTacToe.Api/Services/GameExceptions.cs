namespace TicTacToe.Api.Services;

/// <summary>Thrown for any client-caused invalid request (maps to HTTP 400).</summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}

/// <summary>Thrown when a requested game id does not exist (maps to HTTP 404).</summary>
public class GameNotFoundException : Exception
{
    public GameNotFoundException(string gameId) : base($"Game '{gameId}' was not found.") { }
}
