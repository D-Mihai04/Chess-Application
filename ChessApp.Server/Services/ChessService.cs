using ChessApp.Server.Models;

namespace ChessApp.Server.Services;

public class ChessService
{
    private readonly Dictionary<string, GameState> _games = new();
    
    ///  Game lifecycle
    public GameStateDto NewGame()
    {
        var game = new GameState();
        InitializeBoard(game.Board);
        _games[game.GameId] = game;
        return ToDto(game);
    }

    public GameStateDto? GetGame(string gameId) =>
        _games.TryGetValue(gameId, out var g) ? ToDto(g) : null;


    /// Move execution
    public MoveResult MakeMove(MoveRequest req)
    {
        if (!_games.TryGetValue(req.GameId, out var game))
            return Fail("Game not found");

        if (game.IsCheckmate || game.IsStalemate)
            return Fail("Game is over");

        var piece = game.Board[req.FromRow, req.FromCol];
        if (piece == null)
            return Fail("No piece at that square");

        if (piece.Color != game.CurrentTurn)
            return Fail("It's not your turn");

        var validMoves = GetValidMoves(game, req.FromRow, req.FromCol);
        if (!validMoves.Any(m => m[0] == req.ToRow && m[1] == req.ToCol))
            return Fail("Invalid move");

        ApplyMove(game, req.FromRow, req.FromCol, req.ToRow, req.ToCol);

        //Algebraic notation
        var notation = $"{piece.Type.ToString()[0]}:{ColName(req.FromCol)}{8 - req.FromRow}→{ColName(req.ToCol)}{8 - req.ToRow}";
        game.MoveHistory.Add(notation);

        // Switch turn
        game.CurrentTurn = game.CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

        // Detect check / checkmate / stalemate
        UpdateGameStatus(game);

        return new MoveResult { Valid = true, Message = game.Status, NewState = ToDto(game) };
    }

    
    
    /// Valid moves for a piece
    public List<int[]> GetValidMoves(string gameId, int row, int col)
    {
        if (!_games.TryGetValue(gameId, out var game)) return new();
        return GetValidMoves(game, row, col);
    }

    private List<int[]> GetValidMoves(GameState game, int row, int col)
    {
        var piece = game.Board[row, col];
        if (piece == null) return new();

        var candidates = GetCandidateMoves(game.Board, piece, row, col);

        // Filter out moves that leave own king in check
        return candidates.Where(m => !LeavesKingInCheck(game, row, col, m[0], m[1])).ToList();
    }
    
    // Directions
    private static readonly (int, int)[] rookDirs   = { (-1,0),(1,0),(0,-1),(0,1) };
    private static readonly (int, int)[] bishopDirs = { (-1,-1),(-1,1),(1,-1),(1,1) };
    private static readonly (int, int)[] queenDirs  = { (-1,0),(1,0),(0,-1),(0,1),(-1,-1),(-1,1),(1,-1),(1,1) };

    private List<int[]> GetCandidateMoves(ChessPiece?[,] board, ChessPiece piece, int row, int col, bool skipCastling = false)
    {
        return piece.Type switch
        {
            PieceType.Pawn   => PawnMoves(board, piece, row, col),
            PieceType.Rook   => SlidingMoves(board, piece, row, col, rookDirs),
            PieceType.Bishop => SlidingMoves(board, piece, row, col, bishopDirs),
            PieceType.Queen  => SlidingMoves(board, piece, row, col, queenDirs),
            PieceType.Knight => KnightMoves(board, piece, row, col),
            PieceType.King => KingMoves(board, piece, row, col, skipCastling),
            _                => new()
        };
    }
    
    private List<int[]> SlidingMoves(ChessPiece?[,] board, ChessPiece piece, int row, int col, (int, int)[] dirs)
    {
        var moves = new List<int[]>();
        foreach (var (dr, dc) in dirs)
        {
            int r = row + dr, c = col + dc;
            while (InBounds(r, c))
            {
                if (board[r, c] == null) { moves.Add(new[] { r, c }); }
                else { if (board[r, c]!.Color != piece.Color) moves.Add(new[] { r, c }); break; }
                r += dr; c += dc;
            }
        }
        return moves;
    }

    private List<int[]> KnightMoves(ChessPiece?[,] board, ChessPiece piece, int row, int col)
    {
        var moves = new List<int[]>();
        int[][] offsets = { new[]{-2,-1},new[]{-2,1},new[]{-1,-2},new[]{-1,2},new[]{1,-2},new[]{1,2},new[]{2,-1},new[]{2,1} };
        foreach (var o in offsets)
        {
            int r = row + o[0], c = col + o[1];
            if (InBounds(r, c) && board[r, c]?.Color != piece.Color)
                moves.Add(new[] { r, c });
        }
        return moves;
    }

    private List<int[]> KingMoves(ChessPiece?[,] board, ChessPiece piece, int row, int col, bool skipCastling = false)
    {
        var moves = new List<int[]>();

        // Normal king moves
        int[][] offsets = { new[]{-1,-1},new[]{-1,0},new[]{-1,1},new[]{0,-1},new[]{0,1},new[]{1,-1},new[]{1,0},new[]{1,1} };
        foreach (var o in offsets)
        {
            int r = row + o[0], c = col + o[1];
            if (InBounds(r, c) && board[r, c]?.Color != piece.Color)
                moves.Add(new[] { r, c });
        }

        // Castling
        if (!piece.HasMoved && !skipCastling)
        {
            // Kingside
            var kRook = board[row, 7];
            if (kRook != null && kRook.Type == PieceType.Rook && !kRook.HasMoved)
                if (board[row, 5] == null && board[row, 6] == null)
                    if(!IsSquareUnderAttack(board,row,6,piece.Color) && 
                       !IsSquareUnderAttack(board,row,5,piece.Color))
                    moves.Add(new[] { row, 6 });

            // Queenside
            var qRook = board[row, 0];
            if (qRook != null && qRook.Type == PieceType.Rook && !qRook.HasMoved)
                if (board[row, 1] == null && board[row, 2] == null && board[row, 3] == null)
                    if(!IsSquareUnderAttack(board,row,2,piece.Color) && 
                       !IsSquareUnderAttack(board,row,3,piece.Color))
                    moves.Add(new[] { row, 2 });
        }
        
        

        return moves;
    }

    private List<int[]> PawnMoves(ChessPiece?[,] board, ChessPiece piece, int row, int col)
    {
        var moves = new List<int[]>();
        int dir = piece.Color == PieceColor.White ? -1 : 1;
        int startRow = piece.Color == PieceColor.White ? 6 : 1;

        // Forward one
        if (InBounds(row + dir, col) && board[row + dir, col] == null)
        {
            moves.Add(new[] { row + dir, col });
            // Forward two from start
            if (row == startRow && board[row + 2 * dir, col] == null)
                moves.Add(new[] { row + 2 * dir, col });
        }
        // Diagonal captures
        foreach (int dc in new[] { -1, 1 })
        {
            int nr = row + dir, nc = col + dc;
            if (InBounds(nr, nc) && board[nr, nc] != null && board[nr, nc]!.Color != piece.Color)
                moves.Add(new[] { nr, nc });
        }
        return moves;
    }

    
    
    ///  Check / checkmate / draw / castling logic
    private bool LeavesKingInCheck(GameState game, int fromRow, int fromCol, int toRow, int toCol)
    {
        // Clone board
        var clone = CloneBoard(game.Board);
        var piece = clone[fromRow, fromCol]!;
        clone[toRow, toCol] = piece;
        clone[fromRow, fromCol] = null;
        piece.Row = toRow; piece.Col = toCol;
        return IsKingInCheck(clone, piece.Color);
    }

    //Checks if square is under attack for castling
    private bool IsSquareUnderAttack(ChessPiece?[,] board, int row, int col, PieceColor color)
    {
        var enemy = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var p = board[r, c];
            if (p == null || p.Color != enemy) continue;
            var attacks = GetCandidateMoves(board, p, r, c, skipCastling: true);
            if (attacks.Any(m => m[0] == row && m[1] == col)) return true;
        }
        return false;
    }

    private bool IsKingInCheck(ChessPiece?[,] board, PieceColor color)
    {
        // Find king
        int kr = -1, kc = -1;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                if (board[r, c]?.Type == PieceType.King && board[r, c]?.Color == color)
                {
                    kr = r; kc = c; 
                    
                }

        if (kr == -1) return false;

        // Check if any enemy piece attacks the king
        var enemy = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                var p = board[r, c];
                if (p == null || p.Color != enemy) continue;
                var attacks = GetCandidateMoves(board, p, r, c);
                if (attacks.Any(m => m[0] == kr && m[1] == kc)) return true;
            }
        return false;
    }

    private void UpdateGameStatus(GameState game)
    {
        bool inCheck = IsKingInCheck(game.Board, game.CurrentTurn);
        game.IsCheck = inCheck;

        // Count all valid moves for current player
        bool hasAnyMove = false;
        for (int r = 0; r < 8 && !hasAnyMove; r++)
            for (int c = 0; c < 8 && !hasAnyMove; c++)
            {
                var p = game.Board[r, c];
                if (p != null && p.Color == game.CurrentTurn && GetValidMoves(game, r, c).Count > 0)
                    hasAnyMove = true;
            }

        if (!hasAnyMove)
        {
            if (inCheck) { game.IsCheckmate = true; game.Status = "checkmate"; }
            else         { game.IsStalemate = true; game.Status = "stalemate"; }
        }
        else
        {
            game.Status = inCheck ? "check" : "playing";
        }
    }


    private void ApplyMove(GameState game, int fr, int fc, int tr, int tc)
    {
        var piece = game.Board[fr, fc]!;
        
        //Captured pieces
        var captured = game.Board[tr, tc];
        
        if (captured != null)
        {
            if (captured.Color == PieceColor.White)
                game.BlackCaptured.Add(captured);
            else
                game.WhiteCaptured.Add(captured);
        }
        game.Board[tr, tc] = piece;
        game.Board[fr, fc] = null;
        piece.Row = tr; piece.Col = tc; piece.HasMoved = true;
        bool isKing = piece.Type == PieceType.King;
        
        if (piece.Type == PieceType.Pawn && (tr == 0 || tr == 7))
            piece.Type = PieceType.Queen;

        // Handle castling
        if (isKing && Math.Abs(fc - tc) == 2)
        {
            if (tc == 6)
            {
                game.Board[tr, 5] = game.Board[tr, 7];
                game.Board[tr, 7] = null;
                if (game.Board[tr, 5] != null) { game.Board[tr, 5]!.Col = 5; }
            }
            else if (tc == 2)
            {
                game.Board[tr, 3] = game.Board[tr, 0];
                game.Board[tr, 0] = null;
                if (game.Board[tr, 3] != null) { game.Board[tr, 3]!.Col = 3; }
            }
        }
    }

    private ChessPiece?[,] CloneBoard(ChessPiece?[,] board)
    {
        var clone = new ChessPiece?[8, 8];
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                if (board[r, c] != null)
                    clone[r, c] = new ChessPiece
                    {
                        Type = board[r, c]!.Type,
                        Color = board[r, c]!.Color,
                        Row = board[r, c]!.Row,
                        Col = board[r, c]!.Col,
                        HasMoved = board[r, c]!.HasMoved
                    };
        return clone;
    }

    private void InitializeBoard(ChessPiece?[,] board)
    {
        // Back rows
        PieceType[] backRow = { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
                                PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook };
        for (int c = 0; c < 8; c++)
        {
            board[0, c] = new ChessPiece { Type = backRow[c], Color = PieceColor.Black, Row = 0, Col = c };
            board[7, c] = new ChessPiece { Type = backRow[c], Color = PieceColor.White, Row = 7, Col = c };
            board[1, c] = new ChessPiece { Type = PieceType.Pawn, Color = PieceColor.Black, Row = 1, Col = c };
            board[6, c] = new ChessPiece { Type = PieceType.Pawn, Color = PieceColor.White, Row = 6, Col = c };
        }
    }

    private static bool InBounds(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;
    private static string ColName(int c) => ((char)('a' + c)).ToString();


    /// DTO conversion
    private GameStateDto ToDto(GameState game)
    {

        var pieces = new List<PieceDto>();
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                if (game.Board[r, c] != null)
                    pieces.Add(new PieceDto
                    {
                        Type  = game.Board[r, c]!.Type.ToString().ToLower(),
                        Color = game.Board[r, c]!.Color.ToString().ToLower(),
                        Row   = r, Col = c
                    });

        return new GameStateDto
        {
            GameId      = game.GameId,
            Pieces      = pieces,
            CurrentTurn = game.CurrentTurn.ToString().ToLower(),
            IsCheck     = game.IsCheck,
            IsCheckmate = game.IsCheckmate,
            IsStalemate = game.IsStalemate,
            Status      = game.Status,
            MoveHistory = game.MoveHistory,
            WhiteCaptured = game.WhiteCaptured.Select(p => new PieceDto { Type = p.Type.ToString().ToLower(), Color = p.Color.ToString().ToLower(), Row = p.Row, Col = p.Col }).ToList(),
            BlackCaptured = game.BlackCaptured.Select(p => new PieceDto { Type = p.Type.ToString().ToLower(), Color = p.Color.ToString().ToLower(), Row = p.Row, Col = p.Col }).ToList()
            
        };
    }

    private static MoveResult Fail(string msg) => new() { Valid = false, Message = msg };
}
