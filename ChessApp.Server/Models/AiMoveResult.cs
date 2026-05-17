namespace ChessApp.Server.Models;

public class AiMoveResult
{
    public string Reply { get; set; } = "";
    public MoveResult? MoveResult { get; set; }
}
