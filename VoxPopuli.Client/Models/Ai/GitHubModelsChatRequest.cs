namespace VoxPopuli.Client.Models.Ai;

public sealed class GitHubModelsChatRequest
{
    public string Model { get; set; } = "gpt-4o-mini";

    public List<GitHubModelsChatMessage> Messages { get; set; } = [];

    public float Temperature { get; set; } = 0.7f;

    [System.Text.Json.Serialization.JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 512;
}

public sealed class GitHubModelsChatMessage
{
    public string Role { get; set; } = "user";

    public string Content { get; set; } = string.Empty;
}
