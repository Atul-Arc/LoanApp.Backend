using System.Collections.Concurrent;
using LoanApp.Api.Models;

namespace LoanApp.Api.Services;

/// <summary>
/// Demo in-memory session store.
/// For production, replace with Redis/DB and add eviction/TTL.
/// </summary>
public sealed class InMemoryChatSessionStore : IChatSessionStore
{
 private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new(StringComparer.Ordinal);

 public IReadOnlyList<ChatMessage> GetOrCreate(string sessionId)
 {
 var list = _sessions.GetOrAdd(sessionId, _ => new List<ChatMessage>());
 lock (list)
 {
 return list.ToList();
 }
 }

 public void Append(string sessionId, ChatMessage message)
 {
 var list = _sessions.GetOrAdd(sessionId, _ => new List<ChatMessage>());
 lock (list)
 {
 list.Add(message);
 }
 }

 public void Clear(string sessionId) => _sessions.TryRemove(sessionId, out _);
}
