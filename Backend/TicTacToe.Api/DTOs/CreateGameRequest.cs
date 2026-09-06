using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class CreateGameRequest
{
    /// <summary>Defaults to TwoPlayer when omitted.</summary>
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}
