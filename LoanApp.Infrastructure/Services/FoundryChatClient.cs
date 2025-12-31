using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using LoanApp.Application.Configuration;
using LoanApp.Application.Interfaces;

namespace LoanApp.Infrastructure.Services;

public sealed class FoundryChatClient : IChatService
{
    private readonly FoundryChatOptions _options;

    public FoundryChatClient(IOptions<FoundryChatOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GetReplyAsync(IReadOnlyList<LoanApp.Domain.ChatMessage> history, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("FoundryChat:Endpoint is not configured.");
        if (string.IsNullOrWhiteSpace(_options.Deployment))
            throw new InvalidOperationException("FoundryChat:Deployment is not configured.");
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("FoundryChat:ApiKey is not configured.");

        // OpenAI .NET v2 uses System.ClientModel.ApiKeyCredential.
        var chat = new ChatClient(
            model: _options.Deployment,
            credential: new ApiKeyCredential(_options.ApiKey),
            options: new OpenAIClientOptions { Endpoint = new Uri(_options.Endpoint) });

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new SystemChatMessage(_options.SystemPrompt)
        };

        foreach (var m in history)
        {
            var normalizedRole = m.Role?.ToLowerInvariant() switch
            {
                "system" => "system",
                "assistant" => "assistant",
                _ => "user",
            };

            messages.Add(normalizedRole switch
            {
                "system" => new SystemChatMessage(m.Content),
                "assistant" => new AssistantChatMessage(m.Content),
                _ => new UserChatMessage(m.Content)
            });
        }

        var completion = await chat.CompleteChatAsync(
            messages,
            new ChatCompletionOptions { Temperature = _options.Temperature },
            cancellationToken);

        var reply = completion.Value?.Content?.FirstOrDefault()?.Text;
        return reply ?? string.Empty;
    }
}
