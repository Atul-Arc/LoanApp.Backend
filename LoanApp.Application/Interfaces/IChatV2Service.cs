using LoanApp.Domain;

namespace LoanApp.Application.Interfaces;

public interface IChatV2Service
{
 Task<string> GetReplyAsync(string sessionId, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken);
 void ClearConversation(string sessionId);
}
