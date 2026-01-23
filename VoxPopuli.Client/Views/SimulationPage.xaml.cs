using SkiaSharp;
using SkiaSharp.Views.Maui;
using VoxPopuli.Client.ViewModels;

namespace VoxPopuli.Client.Views;

public partial class SimulationPage : ContentPage
{
    private readonly SimulationViewModel _viewModel;

    // Cache des outils de dessin (Optimisation GC Critique)
    private readonly SKPaint _agentPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true // Lissage des bords
    };

    public SimulationPage(SimulationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Démarrage de la boucle de rendu (Game Loop)
        // 16ms ~= 60 FPS
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            // Demande le redessin du canvas
            SimulationCanvas.InvalidateSurface();

            // Phase 2 : On appellera ici _viewModel.UpdateSimulationLogic();

            return true; // Continuer le timer
        });
    }

    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        SKImageInfo info = e.Info;
        SKSurface surface = e.Surface;
        SKCanvas canvas = surface.Canvas;

        // 1. Effacer l'écran (Fond blanc)
        canvas.Clear(SKColors.White);

        // 2. Calculer le facteur d'échelle
        // Supposons un monde virtuel de 1000x1000
        float scaleX = info.Width / 1000f;
        float scaleY = info.Height / 1000f;

        // Garder le ratio d'aspect pour ne pas déformer les agents
        float scale = Math.Min(scaleX, scaleY);

        // 3. Dessiner les agents
        // Accès direct à la liste (pas de LINQ pour la perf)
        var agents = _viewModel.Population;
        int count = agents.Count;

        for (int i = 0; i < count; i++)
        {
            var agent = agents[i];

            // Mise à jour de la couleur depuis le modèle (pré-calculée)
            _agentPaint.Color = agent.RenderColor;

            // Dessin du cercle (Agent)
            canvas.DrawCircle(
                agent.X * scale,  // Projection X
                agent.Y * scale,  // Projection Y
                5 * scale,        // Rayon ajusté à l'échelle
                _agentPaint
            );
        }
    }
}