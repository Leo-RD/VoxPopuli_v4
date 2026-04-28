using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class PredictionCreateRequest
{
    [JsonPropertyName("predictionId")]
    public int? PredictionId { get; set; }

    [JsonPropertyName("contenu")]
    public string Contenu { get; set; } = string.Empty;

    [JsonPropertyName("reaction")]
    public string Reaction { get; set; } = string.Empty;

    [JsonPropertyName("niveau_emotion")]
    public int NiveauEmotion { get; set; }

    [JsonPropertyName("agentId")]
    public int? AgentId { get; set; }
}
