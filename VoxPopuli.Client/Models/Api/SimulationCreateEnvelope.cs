using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class SimulationCreateEnvelope
{
    [JsonPropertyName("createDto")]
    public SimulationCreateRequest CreateDto { get; set; } = new();
}
