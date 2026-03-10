using MQTTnet;
using MQTTnet.Server;

namespace VoxPopuli.Client.Services;

/// <summary>
/// Broker MQTT embarqué : l'application fait office de serveur.
/// Les clients externes se connectent sur localhost:1883 pour recevoir
/// les données de la simulation en temps réel.
/// Topics disponibles :
///   voxpopuli/agents/snapshot   → snapshot complet (JSON array), publié toutes les 500ms
///   voxpopuli/agents/{id}/state → état d'un agent individuel
/// </summary>
public class MqttBrokerService : IAsyncDisposable
{
    private readonly MqttServer _server;

    public bool IsRunning { get; private set; }

    public MqttBrokerService()
    {
        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883)
            .Build();

        _server = new MqttFactory().CreateMqttServer(options);

        _server.ClientConnectedAsync += e =>
        {
            System.Diagnostics.Debug.WriteLine($"🔌 MQTT: client connecté → {e.ClientId}");
            return Task.CompletedTask;
        };

        _server.ClientDisconnectedAsync += e =>
        {
            System.Diagnostics.Debug.WriteLine($"🔌 MQTT: client déconnecté → {e.ClientId}");
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        try
        {
            await _server.StartAsync();
            IsRunning = true;
            System.Diagnostics.Debug.WriteLine("🟢 Broker MQTT démarré sur localhost:1883");
        }
        catch (Exception ex)
        {
            // Le broker peut échouer sur certaines plateformes (iOS/Android sandbox)
            System.Diagnostics.Debug.WriteLine($"⚠️ Broker MQTT non disponible sur cette plateforme : {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        if (!IsRunning) return;
        await _server.StopAsync();
        IsRunning = false;
        System.Diagnostics.Debug.WriteLine("🔴 Broker MQTT arrêté");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _server.Dispose();
    }
}
