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

    private readonly SKPaint _fpsPaint = new SKPaint
    {
        Color = SKColors.White,
        TextSize = 20,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
    };

    private readonly SKPaint _fpsBackgroundPaint = new SKPaint
    {
        Color = new SKColor(0, 0, 0, 180), // Noir semi-transparent
        Style = SKPaintStyle.Fill
    };

    public SimulationPage(SimulationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Démarrage de la boucle de rendu (Game Loop) optimisée pour 60 FPS
        // Utilisation d'un timer plus précis
        StartRenderLoop();
    }

    private void StartRenderLoop()
    {
        // Timer optimisé pour viser 30 FPS (33.33ms par frame)
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(33.33), () =>
        {
            // Mise à jour de la logique de simulation (Random Walk)
            _viewModel.UpdateSimulationLogic();

            // Demande le redessin du canvas
            SimulationCanvas.InvalidateSurface();

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

        // 4. Dessiner le compteur FPS en haut à droite
        DrawFpsCounter(canvas, info.Width);
    }

    /// <summary>
    /// Dessine le compteur FPS en temps réel.
    /// </summary>
    private void DrawFpsCounter(SKCanvas canvas, int canvasWidth)
    {
        string fpsText = $"FPS: {_viewModel.CurrentFps}";

        // Mesurer la taille du texte
        SKRect textBounds = new SKRect();
        _fpsPaint.MeasureText(fpsText, ref textBounds);

        // Position en haut à DROITE avec un padding
        float padding = 10;
        float rectWidth = textBounds.Width + 20;
        float rectHeight = 30;
        float rectX = canvasWidth - rectWidth - padding; // Aligné à droite
        float rectY = padding;

        // Dessiner le fond semi-transparent
        canvas.DrawRoundRect(rectX, rectY, rectWidth, rectHeight, 5, 5, _fpsBackgroundPaint);

        // Dessiner le texte
        canvas.DrawText(fpsText, rectX + 10, rectY + 22, _fpsPaint);

        // Indicateur de couleur selon les performances (à gauche du badge)
        SKColor indicatorColor = _viewModel.CurrentFps switch
        {
            >= 55 => SKColors.LimeGreen,  // Excellent (>= 55 FPS)
            >= 45 => SKColors.Orange,      // Moyen (45-54 FPS)
            _ => SKColors.Red              // Mauvais (< 45 FPS)
        };

        var indicatorPaint = new SKPaint { Color = indicatorColor, Style = SKPaintStyle.Fill };
        canvas.DrawCircle(rectX - 15, rectY + rectHeight / 2, 5, indicatorPaint);
    }
}