using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Service de transmission des données agents via MQTT.
/// Topic : voxpopuli/agents/{agentId}/state
/// </summary>
public class MqttAgentService : IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    // DTO léger pour la sérialisation (évite d'exposer SKColor non-sérialisable)
    private record AgentStateDto(
        string Id,
        string Name,
        string Group,
        float X,
        float Y,
        bool IsHappy,
        bool IsInfluenced,
        string Emotion,
        float[] OpinionVector
    );

    public bool IsConnected => _client.IsConnected;

    public MqttAgentService(string brokerHost, int port = 1883)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerHost, port)
            .WithClientId($"VoxPopuli_{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (!_client.IsConnected)
            await _client.ConnectAsync(_options, ct);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_client.IsConnected)
            await _client.DisconnectAsync(cancellationToken: ct);
    }

    /// <summary>Publie l'état d'un seul agent.</summary>
    public Task PublishAgentAsync(AgentModel agent, CancellationToken ct = default)
    {
        var dto = ToDto(agent);
        var payload = JsonSerializer.SerializeToUtf8Bytes(dto);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"voxpopuli/agents/{agent.Id}/state")
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
            .WithRetainFlag(false)
            .Build();

        return _client.PublishAsync(message, ct);
    }

    /// <summary>Publie un snapshot de tous les agents en une seule fois.</summary>
    public Task PublishSnapshotAsync(IEnumerable<AgentModel> agents, CancellationToken ct = default)
    {
        var dtos = agents.Select(ToDto);
        var payload = JsonSerializer.SerializeToUtf8Bytes(dtos);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("voxpopuli/agents/snapshot")
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        return _client.PublishAsync(message, ct);
    }

    /// <summary>
    /// Version sécurisée : ne lève pas d'exception si le broker est indisponible.
    /// Utilisé depuis la boucle de simulation.
    /// </summary>
    public async Task TryPublishSnapshotAsync(IEnumerable<AgentModel> agents, CancellationToken ct = default)
    {
        if (!_client.IsConnected) return;
        try
        {
            await PublishSnapshotAsync(agents, ct);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ MQTT publish échoué : {ex.Message}");
        }
    }

    private static AgentStateDto ToDto(AgentModel a) => new(
        a.Id,
        a.Name,
        a.Group,
        a.X,
        a.Y,
        a.IsHappy,
        a.IsInfluenced,
        a.CurrentEmotion.ToString(),
        a.OpinionVector
    );

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _client.Dispose();
    }
}
