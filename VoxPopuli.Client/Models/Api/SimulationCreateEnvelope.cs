using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class SimulationCreateEnvelope
{
    [JsonPropertyName("CreateDto")]
    public SimulationCreateRequest CreateDto { get; set; } = new();
}
