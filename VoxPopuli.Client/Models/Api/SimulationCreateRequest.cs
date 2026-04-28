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

    [JsonPropertyName("NombreAgentsGauche")]
    public int NombreAgentsGauche { get; set; }

    [JsonPropertyName("NombreAgentGauche")]
    public int NombreAgentGauche { get; set; }

    [JsonPropertyName("NbAgentsGauche")]
    public int NbAgentsGauche { get; set; }

    [JsonPropertyName("NbAgentGauche")]
    public int NbAgentGauche { get; set; }

    [JsonPropertyName("Gauche")]
    public int Gauche { get; set; }

    public int RightAgents { get; set; }

    [JsonPropertyName("NombreAgentsDroite")]
    public int NombreAgentsDroite { get; set; }

    [JsonPropertyName("NombreAgentDroite")]
    public int NombreAgentDroite { get; set; }

    [JsonPropertyName("NbAgentsDroite")]
    public int NbAgentsDroite { get; set; }

    [JsonPropertyName("NbAgentDroite")]
    public int NbAgentDroite { get; set; }

    [JsonPropertyName("Droite")]
    public int Droite { get; set; }
    public int HappyAgents { get; set; }
    public int UnhappyAgents { get; set; }

    public string GlobalOrientation { get; set; } = "Neutre";
    public float AverageScore { get; set; }

    public int TotalSentences { get; set; }
    public int LeftSentences { get; set; }
    public int RightSentences { get; set; }
    public int NeutralSentences { get; set; }

    [JsonPropertyName("CentreSentences")]
    public int CentreSentences { get; set; }

    public string SpeechText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("Agents")]
    public List<AgentCreateRequest> Agents { get; set; } = new();
}
