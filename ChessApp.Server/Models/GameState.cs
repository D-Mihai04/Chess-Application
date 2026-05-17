namespace ChessApp.Server.Models;

public class GameState
{
    public string GameId { get; set; } = Guid.NewGuid().ToString();
    public ChessPiece?[,] Board { get; set; } = new ChessPiece?[8, 8];
    public PieceColor CurrentTurn { get; set; } = PieceColor.White;
    public bool IsCheck { get; set; } = false;
    public bool IsCheckmate { get; set; } = false;
    public bool IsStalemate { get; set; } = false;
    public List<string> MoveHistory { get; set; } = new();
    public string Status { get; set; } = "playing";
    public List<ChessPiece> BlackCaptured {get; set;} = new ();
    public List<ChessPiece> WhiteCaptured {get; set;} = new ();
    
}