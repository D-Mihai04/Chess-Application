namespace ChessApp.Server.Models;
public class MoveRequest
{
    public string GameId { get; set; } = "";
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
}
