using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VoxPopuli.Client.Views;
using VoxPopuli.Client.ViewModels;
using VoxPopuli.Client.Services;
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
               // Polices par défaut OpenSans
               fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
               fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

               // Polices ITC Avant Garde Gothic
               fonts.AddFont("ITC Avant Garde Gothic Bold/ITC Avant Garde Gothic Bold.otf", "AvantGardeBold");
               fonts.AddFont("ITC Avant Garde Gothic Bold Condensed/ITC Avant Garde Gothic Bold Condensed.otf", "AvantGardeBoldCondensed");
               fonts.AddFont("ITC Avant Garde Gothic Bold Oblique/ITC Avant Garde Gothic Bold Oblique.otf", "AvantGardeBoldOblique");
               fonts.AddFont("ITC Avant Garde Gothic Book Condensed/ITC Avant Garde Gothic Book Condensed.otf", "AvantGardeBookCondensed");
               fonts.AddFont("ITC Avant Garde Gothic CE Book/ITC Avant Garde Gothic CE Book.otf", "AvantGardeBook");
               fonts.AddFont("ITC Avant Garde Gothic CE Book Oblique/ITC Avant Garde Gothic CE Book Oblique.otf", "AvantGardeBookOblique");
               fonts.AddFont("ITC Avant Garde Gothic CE Demi/ITC Avant Garde Gothic CE Demi.otf", "AvantGardeDemi");
               fonts.AddFont("ITC Avant Garde Gothic CE Demi Oblique/ITC Avant Garde Gothic CE Demi Oblique.otf", "AvantGardeDemiOblique");
               fonts.AddFont("ITC Avant Garde Gothic Demi Condensed/ITC Avant Garde Gothic Demi Condensed.otf", "AvantGardeDemiCondensed");
               fonts.AddFont("ITC Avant Garde Gothic Medium/ITC Avant Garde Gothic Medium.otf", "AvantGardeMedium");
               fonts.AddFont("ITC Avant Garde Gothic Medium Condensed/ITC Avant Garde Gothic Medium Condensed.otf", "AvantGardeMediumCondensed");
               fonts.AddFont("ITC Avant Garde Gothic Medium Oblique/ITC Avant Garde Gothic Medium Oblique.otf", "AvantGardeMediumOblique");
           });

        // --- Enregistrement des Services (Phase 3) ---
        // builder.Services.AddSingleton<IAgentService, AgentService>();
        builder.Services.AddSingleton<OnnxInferenceService>();  // AJOUT ICI
        

        // --- Enregistrement des ViewModels ---
        // Singleton pour la simulation : on veut garder l'état des agents si on change de page
        builder.Services.AddSingleton<SimulationViewModel>();
        builder.Services.AddTransient<StartPageViewModel>();
        // Transient pour la config : on veut un formulaire vierge à chaque fois
        builder.Services.AddTransient<ConfigurationViewModel>();

        // --- Enregistrement des Vues ---
        builder.Services.AddTransient<StartPage>();
        builder.Services.AddSingleton<SimulationPage>();
        builder.Services.AddTransient<ConfigurationPage>();

        return builder.Build();
    }
}