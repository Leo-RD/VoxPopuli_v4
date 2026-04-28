using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class AgentCreateRequest
{
    [JsonPropertyName("agentId")]
    public int? AgentId { get; set; }

    [JsonPropertyName("nom")]
    public string Nom { get; set; } = string.Empty;

    [JsonPropertyName("prenom")]
    public string? Prenom { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("oriantationPolitique")]
    public string? OriantationPolitique { get; set; }

    [JsonPropertyName("simulationId")]
    public int? SimulationId { get; set; }

    [JsonPropertyName("predictions")]
    public List<PredictionCreateRequest> Predictions { get; set; } = new();
}
