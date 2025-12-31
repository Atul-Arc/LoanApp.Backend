using LoanApp.Domain;

namespace LoanApp.Application.Interfaces;

public interface IChatSessionStore
{
 IReadOnlyList<ChatMessage> GetOrCreate(string sessionId);
 void Append(string sessionId, ChatMessage message);
 void Clear(string sessionId);
}
