using LoanApp.Api.Models;

namespace LoanApp.Api.Services;

public interface IChatSessionStore
{
 IReadOnlyList<ChatMessage> GetOrCreate(string sessionId);
 void Append(string sessionId, ChatMessage message);
 void Clear(string sessionId);
}
