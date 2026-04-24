namespace VoxPopuli.Client.Models.Api;

public class SimulationCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string SimulationTime { get; set; } = string.Empty;

    public int AgentCount { get; set; }
    public int LeftAgents { get; set; }
    public int RightAgents { get; set; }
    public int HappyAgents { get; set; }
    public int UnhappyAgents { get; set; }

    public string GlobalOrientation { get; set; } = "Neutre";
    public float AverageScore { get; set; }

    public int TotalSentences { get; set; }
    public int LeftSentences { get; set; }
    public int RightSentences { get; set; }
    public int NeutralSentences { get; set; }

    public string SpeechText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
