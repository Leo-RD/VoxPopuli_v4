using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VoxPopuli.Client.Views;
using VoxPopuli.Client.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace VoxPopuli.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
           .UseMauiApp<App>()
           .UseSkiaSharp() // Enregistre les Handlers natifs (OpenGL/Metal/DirectX) 
           .UseMauiCommunityToolkit() // Initialisation du Toolkit UI
           .ConfigureFonts(fonts =>
           {
               fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
               fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
           });

        // --- Enregistrement des Services (Phase 3) ---
        // builder.Services.AddSingleton<IAgentService, AgentService>();

        // --- Enregistrement des ViewModels ---
        // Singleton pour la simulation : on veut garder l'état des agents si on change de page
        builder.Services.AddSingleton<SimulationViewModel>();
        // Transient pour la config : on veut un formulaire vierge à chaque fois
        builder.Services.AddTransient<ConfigurationViewModel>();

        // --- Enregistrement des Vues ---
        builder.Services.AddSingleton<SimulationPage>();
        builder.Services.AddTransient<ConfigurationPage>();

        return builder.Build();
    }
}