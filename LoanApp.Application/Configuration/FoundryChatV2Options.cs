namespace LoanApp.Application.Configuration;

public sealed class FoundryChatV2Options
{
    public const string SectionName = "FoundryChatV2";

    /// <summary>
    /// Azure AI Foundry Project endpoint.
    /// Example: https://{resource-name}.services.ai.azure.com/api/projects/{project-name}
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Foundry Agent name (as created in Azure AI Foundry).
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// How many messages to keep in history.
    /// </summary>
    public int MaxHistoryMessages { get; init; } = 20;
}
