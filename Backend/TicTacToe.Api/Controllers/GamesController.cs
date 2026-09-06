using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>Create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest? request)
    {
        var mode = request?.Mode ?? Models.GameMode.TwoPlayer;
        var state = _gameService.CreateGame(mode);
        return CreatedAtAction(nameof(GetGame), new { id = state.GameId }, state);
    }

    /// <summary>Get the current state of a game session.</summary>
    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> GetGame(string id)
    {
        try
        {
            return Ok(_gameService.GetGame(id));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>Submit a move for player X or O.</summary>
    [HttpPost("{id}/moves")]
    public ActionResult<GameStateResponse> SubmitMove(string id, [FromBody] MoveRequest request)
    {
        try
        {
            return Ok(_gameService.SubmitMove(id, request));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>Undo the last move (or move pair, in Computer Mode).</summary>
    [HttpPost("{id}/undo")]
    public ActionResult<GameStateResponse> Undo(string id)
    {
        try
        {
            return Ok(_gameService.Undo(id));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>Reset the current game (board/history/status), scoreboard untouched.</summary>
    [HttpPost("{id}/reset")]
    public ActionResult<GameStateResponse> ResetGame(string id)
    {
        try
        {
            return Ok(_gameService.ResetGame(id));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }
}
