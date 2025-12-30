namespace LoanApp.Api.Services;

public sealed class FoundryChatOptions
{
 public const string SectionName = "FoundryChat";

 /// <summary>
 /// Azure AI Foundry / Azure OpenAI endpoint.
 /// Example: https://{resource-name}.openai.azure.com/
 /// </summary>
 public string Endpoint { get; init; } = string.Empty;

 /// <summary>
 /// Model deployment name.
 /// Example: gpt-4o-mini
 /// </summary>
 public string Deployment { get; init; } = string.Empty;

 /// <summary>
 /// API key (if using key auth).
 /// </summary>
 public string ApiKey { get; init; } = string.Empty;

 public string SystemPrompt { get; init; } = "You are a helpful assistant.";

 public int MaxHistoryMessages { get; init; } =20;

 public float Temperature { get; init; } =0.7f;
}
