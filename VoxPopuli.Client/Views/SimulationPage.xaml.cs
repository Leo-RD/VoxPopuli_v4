using SkiaSharp;
using SkiaSharp.Views.Maui;
using VoxPopuli.Client.ViewModels;

namespace VoxPopuli.Client.Views;

[QueryProperty(nameof(AgentCount), "AgentCount")]
public partial class SimulationPage : ContentPage
{
    private readonly SimulationViewModel _viewModel;
    private int _agentCount = 500; // Valeur par défaut

    public int AgentCount
    {
        get => _agentCount;
        set
        {
            _agentCount = value;
            if (_viewModel != null)
            {
                _viewModel.ResetSimulationCommand.Execute((object)value);
            }
        }
    }

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

        // Démarrage de la boucle de rendu (Game Loop) configurée pour 30 FPS
        // Pour une meilleure stabilité
        StartRenderLoop();
    }

    private void StartRenderLoop()
    {
        // Timer configuré pour cibler 30 FPS
        // Utilisation de 30ms au lieu de 33.33ms pour compenser l'overhead du timer
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(30), () =>
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

        // 2. Calculer le facteur d'échelle avec zoom
        // Le monde virtuel fait 1000x1000, on scale pour remplir tout le canvas
        float baseScaleX = info.Width / 1000f;
        float baseScaleY = info.Height / 1000f;

        // Appliquer le zoom
        float scaleX = baseScaleX * _viewModel.ZoomLevel;
        float scaleY = baseScaleY * _viewModel.ZoomLevel;

        // 3. Dessiner les agents
        // Accès direct à la liste (pas de LINQ pour la perf)
        var agents = _viewModel.Population;
        int count = agents.Count;

        for (int i = 0; i < count; i++)
        {
            var agent = agents[i];

            // Mise à jour de la couleur depuis le modèle (pré-calculée)
            _agentPaint.Color = agent.RenderColor;

            // Dessin du cercle (Agent) avec scaling indépendant X/Y pour remplir le canvas
            canvas.DrawCircle(
                agent.X * scaleX,  // Projection X avec scale X et zoom
                agent.Y * scaleY,  // Projection Y avec scale Y et zoom
                4 * _viewModel.ZoomLevel, // Rayon adapté au zoom
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
        // Seuils adaptés pour 30 FPS cible
        SKColor indicatorColor = _viewModel.CurrentFps switch
        {
            >= 28 => SKColors.LimeGreen,  // Excellent (>= 28 FPS, proche de 30)
            >= 22 => SKColors.Orange,      // Moyen (22-27 FPS)
            _ => SKColors.Red              // Mauvais (< 22 FPS)
        };

        var indicatorPaint = new SKPaint { Color = indicatorColor, Style = SKPaintStyle.Fill };
        canvas.DrawCircle(rectX - 15, rectY + rectHeight / 2, 5, indicatorPaint);
    }
}