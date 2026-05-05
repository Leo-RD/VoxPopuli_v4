namespace VoxPopuli.Client.Models.Ai;

public sealed class GitHubModelsChatResponse
{
    public List<GitHubModelsChatChoice> Choices { get; set; } = [];
}

public sealed class GitHubModelsChatChoice
{
    public GitHubModelsChatMessage Message { get; set; } = new();
}
