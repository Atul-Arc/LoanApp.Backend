using LoanApp.Application.Configuration;
using LoanApp.Application.Dtos;
using LoanApp.Application.Interfaces;
using LoanApp.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/v2/chat")]
public sealed class ChatV2Controller : ControllerBase
{
    private readonly ILogger<ChatV2Controller> _logger;
    private readonly IChatSessionStore _store;
    private readonly IChatV2Service _chat;
    private readonly FoundryChatV2Options _options;

    public ChatV2Controller(
    ILogger<ChatV2Controller> logger,
    IChatSessionStore store,
    IChatV2Service chat,
    IOptions<FoundryChatV2Options> options)
    {
        _logger = logger;
        _store = store;
        _chat = chat;
        _options = options.Value;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received v2 chat request for session {SessionId} with message: '{Message}'", request.SessionId, request.Message);

        if (string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest("'sessionId' is required.");

        if (string.IsNullOrWhiteSpace(request.User))
            return BadRequest("'user' is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("'message' is required.");

        var sessionId = request.SessionId.Trim();
        var userText = request.Message.Trim();

        _store.Append(sessionId, new ChatMessage("user", userText));

        IReadOnlyList<ChatMessage> history = _store.GetOrCreate(sessionId)
        .TakeLast(Math.Max(0, _options.MaxHistoryMessages))
        .ToArray();

        var reply = await _chat.GetReplyAsync(sessionId, history, cancellationToken);

        _store.Append(sessionId, new ChatMessage("assistant", reply));

        return Ok(new ChatResponse(
        SessionId: sessionId,
        Reply: reply,
        TimestampUtc: DateTimeOffset.UtcNow));
    }

    [HttpDelete("{sessionId}")]
    public IActionResult ClearSession([FromRoute] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest("'sessionId' is required.");

        sessionId = sessionId.Trim();
        _store.Clear(sessionId);
        _chat.ClearConversation(sessionId);

        return NoContent();
    }
}
