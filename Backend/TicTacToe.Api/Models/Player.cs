using System.Text.Json.Serialization;

namespace TicTacToe.Api.Models;

/// <summary>
/// The two possible players in a game of Tic Tac Toe.
/// Serialized as a string ("X" / "O") so the API contract stays readable.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Player
{
    X,
    O
}
