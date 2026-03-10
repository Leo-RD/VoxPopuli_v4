using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Client MQTT qui se connecte au broker Mosquitto sur la Raspberry Pi du collègue.
///
/// Topics :
///   vox/vers/ras  → ce que l'app ENVOIE  (snapshot des agents → Raspberry)
///   vox/vers/app  → ce que l'app REÇOIT  (commandes/events → depuis la Raspberry)
/// </summary>
public class MqttAgentService : IAsyncDisposable
{
    // ── Topics (fournis par le collègue) ──────────────────────────────────────
    public const string TopicToRaspberry   = "vox/vers/ras";
    public const string TopicFromRaspberry = "vox/vers/app";

    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    // ── DTO léger (évite d'exposer SKColor non-sérialisable) ─────────────────
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

    /// <summary>Déclenché à chaque message reçu de la Raspberry (payload brut).</summary>
    public event Action<string>? MessageFromRaspberryReceived;

    public bool IsConnected => _client.IsConnected;

    public MqttAgentService(string brokerIp, int port = 1883)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerIp, port)
            .WithClientId($"VoxPopuliApp_{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();

        // Branchement du handler de réception
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    /// <summary>Connexion au broker + abonnement au topic entrant.</summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_client.IsConnected) return;
        try
        {
            await _client.ConnectAsync(_options, ct);
            await _client.SubscribeAsync(TopicFromRaspberry, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, ct);
            System.Diagnostics.Debug.WriteLine($"🟢 MQTT connecté | abonné à '{TopicFromRaspberry}'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ MQTT connexion échouée : {ex.Message}");
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_client.IsConnected)
            await _client.DisconnectAsync(cancellationToken: ct);
    }

    // ── Publication ───────────────────────────────────────────────────────────

    /// <summary>
    /// Publie les données d'un agent unique vers la Raspberry (topic vox/vers/ras).
    /// Appelé au clic sur un agent dans le canvas.
    /// </summary>
    public async Task TryPublishAgentAsync(AgentModel agent, CancellationToken ct = default)
    {
        if (!_client.IsConnected) return;
        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(ToDto(agent));
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(TopicToRaspberry)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            await _client.PublishAsync(message, ct);
            System.Diagnostics.Debug.WriteLine($"📤 MQTT -> {agent.Name} envoyé sur '{TopicToRaspberry}'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ MQTT publish échoué : {ex.Message}");
        }
    }

    // ── Réception ─────────────────────────────────────────────────────────────

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = e.ApplicationMessage.ConvertPayloadToString();
        System.Diagnostics.Debug.WriteLine($"📩 MQTT [{e.ApplicationMessage.Topic}] : {payload}");
        MessageFromRaspberryReceived?.Invoke(payload);
        return Task.CompletedTask;
    }

    // ── Sérialisation ─────────────────────────────────────────────────────────

    private static AgentStateDto ToDto(AgentModel a) => new(
        a.Id, a.Name, a.Group, a.X, a.Y,
        a.IsHappy, a.IsInfluenced,
        a.CurrentEmotion.ToString(),
        a.OpinionVector
    );

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _client.Dispose();
    }
}
