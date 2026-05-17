using System.Text;
using System.Text.Json;
using ChessApp.Server.Models;

namespace ChessApp.Server.Services;

public class AiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ChessService _chess;

    public AiService(HttpClient http, IConfiguration config, ChessService chess)
    {
        _http   = http;
        _config = config;
        _chess  = chess;
    }

    public async Task<AiMoveResult> ProcessMessage(AiMoveRequest req)
    {
        var state = _chess.GetGame(req.GameId);
        if (state == null) return new AiMoveResult { Reply = "Game not found." };

        var boardDesc = string.Join(", ",
            state.Pieces.Select(p => $"{p.Color} {p.Type} at {(char)('a'+p.Col)}{8-p.Row} (row:{p.Row},col:{p.Col})"));

        var systemPrompt = "You are a chess assistant. Parse the user's move and respond with EXACTLY this JSON:\n" +
                           "{ \"reply\": \"Your friendly explanation\", \"move\": { \"fromRow\": 6, \"fromCol\": 4, \"toRow\": 4, \"toCol\": 4 } }\n" +
                           "If no move is requested, omit the move field.\n" +
                           "IMPORTANT coordinate system: Row 0=rank8 (black back row), Row 1=rank7 (black pawns START here), Row 6=rank2 (white pawns START here), Row 7=rank1 (white back row).\n" +
                           "Cols: 0=a, 1=b, 2=c, 3=d, 4=e, 5=f, 6=g, 7=h.\n" +
                           "Example: white pawn e2 to e4 = fromRow:6 fromCol:4 toRow:4 toCol:4\n" +
                           "Example: white knight g1 to f3 = fromRow:7 fromCol:6 toRow:5 toCol:5\n" +
                           "Use the board piece positions provided to find the correct fromRow and fromCol.\n" +
                           "Respond ONLY with valid JSON. No markdown, no extra text.";

        var userContent = $"Board: {boardDesc}\nCurrent turn: {state.CurrentTurn}\nUser says: {req.Message}";

        var apiKey = _config["Groq:ApiKey"] ?? "";

        var body = JsonSerializer.Serialize(new
        {
            model    = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userContent  }
            },
            max_tokens  = 500,
            temperature = 0
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        try
        {
            var response    = await _http.SendAsync(request);
            var raw         = await response.Content.ReadAsStringAsync();
            Console.WriteLine("GROQ RESPONSE: " + raw);

            using var doc   = JsonDocument.Parse(raw);
            var textContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            textContent = textContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            using var parsed = JsonDocument.Parse(textContent);
            var reply        = parsed.RootElement.GetProperty("reply").GetString() ?? "Okay!";

            MoveResult? moveResult = null;
            if (parsed.RootElement.TryGetProperty("move", out var moveEl))
            {
                var moveReq = new MoveRequest
                {
                    GameId  = req.GameId,
                    FromRow = moveEl.GetProperty("fromRow").GetInt32(),
                    FromCol = moveEl.GetProperty("fromCol").GetInt32(),
                    ToRow   = moveEl.GetProperty("toRow").GetInt32(),
                    ToCol   = moveEl.GetProperty("toCol").GetInt32()
                };
                moveResult = _chess.MakeMove(moveReq);
            }

            return new AiMoveResult { Reply = reply, MoveResult = moveResult };
        }
        catch (Exception ex)
        {
            return new AiMoveResult
            {
                Reply = $"I couldn't process that. Error: {ex.Message}"
            };
        }
    }
}