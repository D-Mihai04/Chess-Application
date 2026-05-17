namespace ChessApp.Server.Models;


public enum PieceType
{
    King, Queen, Rook, Bishop, Knight, Pawn
}

public enum PieceColor
{
    White, Black
}

public class ChessPiece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public bool HasMoved { get; set; } = false;
}