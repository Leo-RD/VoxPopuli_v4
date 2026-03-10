using Microsoft.Extensions.DependencyInjection;
using VoxPopuli.Client.Services;

namespace VoxPopuli.Client;

public partial class App : Application
{
	public App(MqttBrokerService mqttBroker)
	{
		InitializeComponent();
		// Démarrer le broker MQTT dès le lancement de l'app
		Task.Run(async () => await mqttBroker.StartAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}