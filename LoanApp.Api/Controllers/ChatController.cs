using LoanApp.Api.Models;
using LoanApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatSessionStore _store;
    private readonly FoundryChatClient _chat;
    private readonly FoundryChatOptions _options;

    public ChatController(ILogger<ChatController> logger, IChatSessionStore store, FoundryChatClient chat, Microsoft.Extensions.Options.IOptions<FoundryChatOptions> options)
    {
        _logger = logger;
        _store = store;
        _chat = chat;
        _options = options.Value;
    }

    /// <summary>
    /// Chat endpoint that keeps context by session id.
    /// The frontend should generate and reuse a stable sessionId (e.g., UUID) per conversation.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received chat request for session {SessionId} with message: '{Message}'", request.SessionId, request.Message);

        if (string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest("'sessionId' is required.");

        if (string.IsNullOrWhiteSpace(request.User))
            return BadRequest("'user' is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("'message' is required.");

        var sessionId = request.SessionId.Trim();
        var userText = request.Message.Trim();

        // Append user message first
        _store.Append(sessionId, new ChatMessage("user", userText));

        // Load capped history (including the new user message)
        IReadOnlyList<ChatMessage> history = _store.GetOrCreate(sessionId)
            .TakeLast(Math.Max(0, _options.MaxHistoryMessages))
            .ToArray();

        var reply = await _chat.GetReplyAsync(history, cancellationToken);
        _logger.LogInformation("Reply from chat service for session {SessionId}: '{Reply}'", sessionId, reply);

        // Append assistant reply
        _store.Append(sessionId, new ChatMessage("assistant", reply));

        var response = new ChatResponse(
            SessionId: sessionId,
            Reply: reply,
            TimestampUtc: DateTimeOffset.UtcNow);
        
        _logger.LogInformation("Sending response for session {SessionId}", sessionId);
        return Ok(response);
    }

    [HttpDelete("{sessionId}")]
    public IActionResult ClearSession([FromRoute] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest("'sessionId' is required.");

        _store.Clear(sessionId.Trim());
        return NoContent();
    }

    public sealed record ChatRequest(string SessionId, string User, string Message);

    public sealed record ChatResponse(string SessionId, string Reply, DateTimeOffset TimestampUtc);
}
