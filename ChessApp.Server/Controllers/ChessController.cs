using ChessApp.Server.Models;
using ChessApp.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChessApp.Server.Controllers;

[ApiController]
[Route("api/chess")]
public class ChessController : ControllerBase
{
    private readonly ChessService _chess;
    private readonly AiService    _ai;

    public ChessController(ChessService chess, AiService ai)
    {
        _chess = chess;
        _ai    = ai;
    }

    ///Start a new chess game. Returns initial board state with a gameId.
    [HttpPost("new")]
    public IActionResult NewGame() => Ok(_chess.NewGame());

    ///Get current game state by gameId.
    [HttpGet("{gameId}")]
    public IActionResult GetGame(string gameId)
    {
        var state = _chess.GetGame(gameId);
        return state is null ? NotFound("Game not found") : Ok(state);
    }

    ///Execute a move. Returns updated board state.
    [HttpPost("move")]
    public IActionResult MakeMove([FromBody] MoveRequest req)
    {
        var result = _chess.MakeMove(req);
        return Ok(result);
    }

    ///Get all valid destination squares for a piece at (row, col).
    [HttpPost("valid-moves")]
    public IActionResult GetValidMoves([FromBody] ValidMovesRequest req)
    {
        var moves = _chess.GetValidMoves(req.GameId, req.Row, req.Col);
        return Ok(new ValidMovesResult { Moves = moves });
    }

    ///Send a message to the AI chess assistant.
    [HttpPost("ai-move")]
    public async Task<IActionResult> AiMove([FromBody] AiMoveRequest req)
    {
        var result = await _ai.ProcessMessage(req);
        return Ok(result);
    }
}
