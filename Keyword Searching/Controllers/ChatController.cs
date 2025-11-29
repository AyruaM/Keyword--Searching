using Microsoft.AspNetCore.Mvc;
using OllamaSharp.Models.Chat;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly OllamaChatService _svc;

    // Memory storage for multi-turn chats
    private static readonly Dictionary<string, List<Dictionary<string, string>>> Sessions
        = new();

    public ChatController(OllamaChatService svc)
    {
        _svc = svc;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest req)
    {
        if (!Sessions.ContainsKey(req.SessionId))
            Sessions[req.SessionId] = new List<Dictionary<string, string>>();

        var history = Sessions[req.SessionId];

        // Add user message
        history.Add(new Dictionary<string, string>
        {
            { "role", "user" },
            { "content", req.Message }
        });

        Console.WriteLine("🟦 USER:");
        Console.WriteLine(req.Message);

        var responseText = await _svc.ChatAsync(history);

        Console.WriteLine("🟩 DEEPSEEK:");
        Console.WriteLine(responseText);

        // Add assistant reply
        history.Add(new Dictionary<string, string>
        {
            { "role", "assistant" },
            { "content", responseText }
        });

        return Ok(new { response = responseText });
    }

    [HttpPost("reset")]
    public IActionResult Reset([FromBody] ResetRequest req)
    {
        Sessions.Remove(req.SessionId);
        return Ok(new { message = "Chat history cleared" });
    }
    public class ChatRequest
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; }
    }
    public class ResetRequest
    {
        public string SessionId { get; set; }
    }

}
