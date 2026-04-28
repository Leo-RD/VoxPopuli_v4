using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class SimulationCreateRequest
{
    [JsonPropertyName("simulationId")]
    public int SimulationId { get; set; }

    [JsonPropertyName("Titre")]
    public string Titre { get; set; } = string.Empty;

    [JsonPropertyName("Discours")]
    public string Discours { get; set; } = string.Empty;

    [JsonPropertyName("TypeTest")]
    public string TypeTest { get; set; } = string.Empty;

    [JsonPropertyName("NbAgent")]
    public int NbAgent { get; set; }

    [JsonPropertyName("DateSimulation")]
    public DateTime DateSimulation { get; set; }

    [JsonPropertyName("Agents")]
    public List<AgentCreateRequest> Agents { get; set; } = new();
}
