namespace LoanApp.Api.Models;

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatRequest(
 string SessionId,
 string User,
 string Message);

public sealed record ChatResponse(
 string SessionId,
 string Reply,
 DateTimeOffset TimestampUtc);
