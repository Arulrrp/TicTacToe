using System.Text.Json.Serialization;

namespace TicTacToe.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameStatus
{
    InProgress,
    Won,
    Draw
}
