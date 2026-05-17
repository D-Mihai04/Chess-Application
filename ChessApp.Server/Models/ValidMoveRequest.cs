namespace ChessApp.Server.Models;

public class ValidMovesRequest
{
    public string GameId { get; set; } = "";
    public int Row { get; set; }
    public int Col { get; set; }
}