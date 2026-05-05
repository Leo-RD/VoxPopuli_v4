namespace VoxPopuli.Client.Services.Ai;

public sealed class GitHubModelsOptions
{
    public string ApiKeyFilePath { get; set; } = "";

    public string Model { get; set; } = "gpt-4o-mini";

    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com/chat/completions";
}
