namespace ChessApp.Server.Models;

public class GameStateDto
{
    public string GameId { get; set; } = "";
    public List<PieceDto> Pieces { get; set; } = new();
    public string CurrentTurn { get; set; } = "white";
    public bool IsCheck { get; set; }
    public bool IsCheckmate { get; set; }
    public bool IsStalemate { get; set; }
    public string Status { get; set; } = "playing";
    public List<string> MoveHistory { get; set; } = new();
    
    public List<PieceDto> WhiteCaptured { get; set; } = new();
    public List<PieceDto> BlackCaptured { get; set; } = new();
}
