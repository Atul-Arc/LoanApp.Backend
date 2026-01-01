using System.Collections.Concurrent;
using System.Text;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using LoanApp.Application.Configuration;
using LoanApp.Application.Interfaces;
using LoanApp.Domain;
using Microsoft.Extensions.Options;

namespace LoanApp.Infrastructure.Services;

/// <summary>
/// Chat V2 implementation that talks to an Azure AI Foundry Agent via Azure AI Projects SDK.
/// 
/// Note on context:
/// The Azure.AI.Projects SDK version referenced by this solution does not currently expose
/// the same "conversation" API shape shown in the Python example (create conversation -> reuse id).
/// 
/// To preserve follow-up context, we concatenate the last N messages into a single prompt.
/// If/when conversation APIs are available in the .NET SDK, this can be upgraded to true
/// server-side conversations.
/// </summary>
public sealed class FoundryChatV2Client : IChatV2Service
{
    private readonly FoundryChatV2Options _options;

    // Reserved for future: sessionId -> conversationId mapping
    private readonly ConcurrentDictionary<string, string> _conversationIds = new(StringComparer.Ordinal);

    public FoundryChatV2Client(IOptions<FoundryChatV2Options> options)
    {
        _options = options.Value;
    }

    public void ClearConversation(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        _conversationIds.TryRemove(sessionId, out _);
    }

    public Task<string> GetReplyAsync(string sessionId, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("FoundryChatV2:Endpoint is not configured.");
        if (string.IsNullOrWhiteSpace(_options.AgentName))
            throw new InvalidOperationException("FoundryChatV2:AgentName is not configured.");
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId is required.", nameof(sessionId));

        var projectClient = new AIProjectClient(new Uri(_options.Endpoint), new DefaultAzureCredential());

        var agentRecord = projectClient.Agents.GetAgent(_options.AgentName).Value;
        var agentRef = new AgentReference(agentRecord.Id);

        // Build a single prompt that includes the recent conversation.
        // This is a pragmatic substitute for server-side conversations.
        var prompt = BuildPrompt(history);

        ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentRef);
        var response = responseClient.CreateResponse(prompt);

        var text = response.Value.GetOutputText();
        return Task.FromResult(text ?? string.Empty);
    }

    private static string BuildPrompt(IReadOnlyList<ChatMessage> history)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant. Use the conversation context below to answer the user's last question.");
        sb.AppendLine();
        sb.AppendLine("Conversation:");

        foreach (var m in history)
        {
            var role = string.IsNullOrWhiteSpace(m.Role) ? "user" : m.Role.Trim().ToLowerInvariant();
            sb.Append(role switch
            {
                "assistant" => "Assistant",
                "system" => "System",
                _ => "User"
            });
            sb.Append(": ");
            sb.AppendLine(m.Content ?? string.Empty);
        }

        sb.AppendLine();
        sb.AppendLine("Answer:");
        return sb.ToString();
    }
}
