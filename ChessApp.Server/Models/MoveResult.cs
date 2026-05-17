namespace ChessApp.Server.Models;

public class MoveResult
{
    public bool Valid { get; set; }
    public string Message { get; set; } = "";
    public GameStateDto? NewState { get; set; }
}