using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IGameService _gameService;

    public ScoreboardController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>Get the session-level scoreboard (X wins / O wins / draws).</summary>
    [HttpGet]
    public ActionResult<ScoreboardResponse> Get()
    {
        return Ok(_gameService.GetScoreboard());
    }

    /// <summary>Reset the scoreboard back to zero. Does not affect any in-progress game.</summary>
    [HttpPost("reset")]
    public ActionResult<ScoreboardResponse> Reset()
    {
        return Ok(_gameService.ResetScoreboard());
    }
}
