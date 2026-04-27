using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class SimulationCreateRequest
{
    [JsonPropertyName("Titre")]
    public string Titre { get; set; } = string.Empty;

    [JsonPropertyName("Discours")]
    public string Discours { get; set; } = string.Empty;

    [JsonPropertyName("TypeTest")]
    public string TypeTest { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string SimulationTime { get; set; } = string.Empty;

    public int AgentCount { get; set; }

    [JsonPropertyName("NombreAgents")]
    public int NombreAgents { get; set; }

    [JsonPropertyName("NombreAgent")]
    public int NombreAgent { get; set; }

    [JsonPropertyName("NbAgents")]
    public int NbAgents { get; set; }

    [JsonPropertyName("NbAgent")]
    public int NbAgent { get; set; }

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
