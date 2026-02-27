using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VoxPopuli.Client.Models;
using VoxPopuli.Client.Services;
using System.Linq;

namespace VoxPopuli.Client.ViewModels;

public partial class SimulationViewModel : BaseViewModel
{
    private readonly OnnxInferenceService _onnxService;
    private readonly Random _random = new Random();

    // Constantes pour la simulation
    private const float WorldWidth = 1000f;
    private const float WorldHeight = 1000f;
    private const float DirectionChangeChance = 0.05f; // 5% de chance de changer de direction par frame

    // Tracking du FPS
    private DateTime _lastFrameTime = DateTime.Now;
    private int _frameCount = 0;
    private double _accumulatedTime = 0;

    [ObservableProperty]
    private int currentFps = 60;

    public List<AgentModel> Population { get; private set; } = new();

    [ObservableProperty]
    private int agentCount;

    [ObservableProperty]
    private bool isRunning = true; // État de la simulation

    [ObservableProperty]
    private string simulationButtonText = "⏸ Arrêter";

    [ObservableProperty]
    private Color simulationButtonColor = Color.FromArgb("#E74C3C");

    [ObservableProperty]
    private float zoomLevel = 1.0f; // Niveau de zoom (1.0 = 100%)

    public SimulationViewModel(OnnxInferenceService onnxService)
    {
        System.Diagnostics.Debug.WriteLine("📊 SimulationViewModel: Initialisation...");
        _onnxService = onnxService;
        Task.Run(async () => await _onnxService.InitializeAsync());
        InitializePopulation(500); // Réduit à 400 pour plus de stabilité à 30 FPS ; si changement ici changer ligne 57
        System.Diagnostics.Debug.WriteLine($"📊 SimulationViewModel: {Population.Count} agents créés");
    }

    /// <summary>
    /// Réinitialise la simulation avec un nombre spécifié d'agents
    /// </summary>
    [RelayCommand]
    private void ResetSimulation(object parameter = null)
    {
        int count = 500; // Par défaut

        if (parameter is int paramCount)
        {
            count = paramCount;
        }

        InitializePopulation(count);
    }

    /// <summary>
    /// Augmente le niveau de zoom
    /// </summary>
    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel + 0.1f, 3.0f); // Max 300%
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom In: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Diminue le niveau de zoom
    /// </summary>
    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel - 0.1f, 0.5f); // Min 50%
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom Out: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Réinitialise le zoom à 100%
    /// </summary>
    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0f;
        System.Diagnostics.Debug.WriteLine($"🔍 Zoom Reset: 100%");
    }

    /// <summary>
    /// Déclenche un événement (changement de couleur des agents)
    /// </summary>
    [RelayCommand]
    private void TriggerEvent()
    {
        // Exemple : Changer l'état émotionnel et la couleur
        foreach (var agent in Population)
        {
            agent.CurrentEmotion = EmotionalState.Agitated;
            agent.RenderColor = SKColors.Red;
        }
    }

    /// <summary>
    /// Diffuse le Message B (exemple : couleur violette)
    /// </summary>
    [RelayCommand]
    private void BroadcastMessageB()
    {
        foreach (var agent in Population)
        {
            agent.CurrentEmotion = EmotionalState.Happy;
            agent.RenderColor = SKColors.Purple;
        }
    }

    /// <summary>
    /// Arrête ou redémarre la simulation
    /// </summary>
    [RelayCommand]
    private void ToggleSimulation()
    {
        IsRunning = !IsRunning;

        // Mise à jour du texte et de la couleur du bouton
        if (IsRunning)
        {
            SimulationButtonText = "⏸ Arrêter";
            SimulationButtonColor = Color.FromArgb("#E74C3C"); // Rouge
        }
        else
        {
            SimulationButtonText = "▶ Reprendre";
            SimulationButtonColor = Color.FromArgb("#27AE60"); // Vert
        }

        System.Diagnostics.Debug.WriteLine($"🎮 Simulation: {(IsRunning ? "RUNNING" : "PAUSED")}");
    }

    /// <summary>
    /// Exécute l'inférence ONNX sur tous les agents
    /// </summary>
    [RelayCommand]
    private async Task RunInferenceAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🧠 Démarrage de l'inférence sur {Population.Count} agents...");

            // Traitement optimisé par batch
            var inputVectors = Population.Select(a => a.OpinionVector).ToArray();
            System.Diagnostics.Debug.WriteLine($"   - Vecteurs d'entrée préparés");

            var predictions = _onnxService.PredictBatch(inputVectors);
            System.Diagnostics.Debug.WriteLine($"   - Prédictions reçues");

            // Mise à jour des agents avec les prédictions
            for (int i = 0; i < Population.Count; i++)
            {
                var agent = Population[i];
                var oldVector = agent.OpinionVector[0];
                agent.OpinionVector = predictions[i];
                agent.RenderColor = GetColorFromOpinion(predictions[i][0]);

                // Log des premiers agents pour vérification
                if (i < 3)
                {
                    System.Diagnostics.Debug.WriteLine($"   Agent {i}: {oldVector:F2} -> {predictions[i][0]:F2}, Couleur: {agent.RenderColor}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Inférence terminée avec succès!");

            // Forcer la notification de changement
            OnPropertyChanged(nameof(Population));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur inférence: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }

    private SKColor GetColorFromOpinion(float opinionScore)
    {
        // Gradient de couleur basé sur le score d'opinion
        if (opinionScore > 0.6f) return SKColors.Green;
        if (opinionScore < 0.4f) return SKColors.Red;
        return SKColors.Orange;
    }

    // ========== MOTEUR DE DÉPLACEMENT (RANDOM WALK) ==========

    /// <summary>
    /// Met à jour la logique de simulation (appelé chaque frame).
    /// Implémente le Random Walk pour chaque agent.
    /// </summary>
    public void UpdateSimulationLogic()
    {
        if (!IsRunning) return;

        // Calcul du FPS
        var currentTime = DateTime.Now;
        var deltaTime = (currentTime - _lastFrameTime).TotalSeconds;
        _lastFrameTime = currentTime;

        _frameCount++;
        _accumulatedTime += deltaTime;

        // Mise à jour du FPS toutes les 30 frames (pour éviter les fluctuations)
        if (_frameCount >= 30)
        {
            CurrentFps = (int)Math.Round(_frameCount / _accumulatedTime);
            _frameCount = 0;
            _accumulatedTime = 0;
        }

        // Parcourir tous les agents pour mettre à jour leur position
        for (int i = 0; i < Population.Count; i++)
        {
            var agent = Population[i];
            UpdateAgentMovement(agent);
        }
    }

    /// <summary>
    /// Met à jour le mouvement d'un agent selon le Random Walk.
    /// </summary>
    private void UpdateAgentMovement(AgentModel agent)
    {
        // 1. Changement aléatoire de direction (Random Walk)
        if (_random.NextDouble() < DirectionChangeChance)
        {
            // Nouvelle direction aléatoire (en radians)
            agent.Direction = (float)(_random.NextDouble() * Math.PI * 2);
        }

        // 2. Calculer le facteur de vitesse selon l'émotion
        float speedMultiplier = agent.CurrentEmotion switch
        {
            EmotionalState.Agitated => 2.5f,  // Très rapide
            EmotionalState.Fearful => 3.0f,   // Fuite rapide
            EmotionalState.Happy => 1.2f,     // Légèrement plus rapide
            EmotionalState.Angry => 1.8f,     // Rapide
            EmotionalState.Neutral => 1.0f,   // Vitesse normale
            _ => 1.0f
        };

        // 3. Calculer la nouvelle vitesse basée sur la direction
        float baseSpeed = agent.MaxSpeed * speedMultiplier;

        // Optimisation : Utiliser MathF pour de meilleures performances
        agent.VelocityX = MathF.Cos(agent.Direction) * baseSpeed;
        agent.VelocityY = MathF.Sin(agent.Direction) * baseSpeed;

        // 4. Mettre à jour la position
        agent.X += agent.VelocityX;
        agent.Y += agent.VelocityY;

        // 5. Gestion des rebonds sur les bords
        HandleBoundaryCollision(agent);
    }

    /// <summary>
    /// Gère les collisions avec les bords de la carte (rebonds).
    /// </summary>
    private void HandleBoundaryCollision(AgentModel agent)
    {
        bool hasCollided = false;

        // Rebond sur le bord gauche ou droit
        if (agent.X < 0)
        {
            agent.X = 0;
            agent.VelocityX = -agent.VelocityX; // Inversion de la direction horizontale
            hasCollided = true;
        }
        else if (agent.X > WorldWidth)
        {
            agent.X = WorldWidth;
            agent.VelocityX = -agent.VelocityX;
            hasCollided = true;
        }

        // Rebond sur le bord haut ou bas
        if (agent.Y < 0)
        {
            agent.Y = 0;
            agent.VelocityY = -agent.VelocityY; // Inversion de la direction verticale
            hasCollided = true;
        }
        else if (agent.Y > WorldHeight)
        {
            agent.Y = WorldHeight;
            agent.VelocityY = -agent.VelocityY;
            hasCollided = true;
        }

        // Recalculer la direction après un rebond
        if (hasCollided)
        {
            agent.Direction = MathF.Atan2(agent.VelocityY, agent.VelocityX);
        }
    }

    private void InitializePopulation(int count)
    {
        var random = new Random();
        Population.Clear();
        AgentCount = count;

        for (int i = 0; i < count; i++)
        {
            // Direction initiale aléatoire
            float initialDirection = (float)(random.NextDouble() * Math.PI * 2);

            Population.Add(new AgentModel
            {
                X = random.Next(0, 1000),
                Y = random.Next(0, 1000),
                OpinionVector = new float[5]
                {
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()
                },
                CurrentEmotion = EmotionalState.Neutral,
                RenderColor = SKColors.BlueViolet,
                Direction = initialDirection,
                MaxSpeed = 1.5f + (float)random.NextDouble() * 1.0f, // Vitesse variable entre 1.5 et 2.5
                Group = i < count / 2 ? "A" : "B" // Répartition 50/50 entre groupes A et B
            });
        }
    }
}