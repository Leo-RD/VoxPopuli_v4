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

    public List<AgentModel> Population { get; private set; } = new();

    [ObservableProperty]
    private int agentCount;

    [ObservableProperty]
    private bool isRunning = true; // État de la simulation

    public SimulationViewModel(OnnxInferenceService onnxService)
    {
        System.Diagnostics.Debug.WriteLine("📊 SimulationViewModel: Initialisation...");
        _onnxService = onnxService;
        Task.Run(async () => await _onnxService.InitializeAsync());
        InitializePopulation(500);
        System.Diagnostics.Debug.WriteLine($"📊 SimulationViewModel: {Population.Count} agents créés");
    }

    /// <summary>
    /// Réinitialise la simulation avec 500 agents
    /// </summary>
    [RelayCommand]
    private void ResetSimulation()
    {
        InitializePopulation(500);
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
        // Vous pouvez ajouter une logique pour pause/resume
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

    private void InitializePopulation(int count)
    {
        var random = new Random();
        Population.Clear();
        AgentCount = count;

        for (int i = 0; i < count; i++)
        {
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
                RenderColor = SKColors.BlueViolet
            });
        }
    }
}