using System.Text.Json.Serialization;

namespace VoxPopuli.Client.Models.Api;

public class AgentCreateRequest
{
    [JsonPropertyName("Nom")]
    public string Nom { get; set; } = string.Empty;

    [JsonPropertyName("Prenom")]
    public string Prenom { get; set; } = string.Empty;

    [JsonPropertyName("Avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("NiveauEmotion")]
    public int NiveauEmotion { get; set; }

    [JsonPropertyName("DateCreation")]
    public DateTime DateCreation { get; set; }

    [JsonPropertyName("OrientationPolitique")]
    public string OrientationPolitique { get; set; } = "Centre";
}
