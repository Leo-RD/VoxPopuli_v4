using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VoxPopuli.Client.Models;
using VoxPopuli.Client.Services;

namespace VoxPopuli.Client.ViewModels;

// CORRECTION 1 : Ajout du mot-clé 'partial' ici

public partial class SimulationViewModel : BaseViewModel
{
    private readonly OnnxInferenceService _onnxService; // AJOUT

    public List<AgentModel> Population { get; private set; } = new();

    [ObservableProperty]
    private int agentCount;

    // Injection de dépendance
    public SimulationViewModel(OnnxInferenceService onnxService)
    {
        _onnxService = onnxService;

        // Initialiser le modèle ONNX de manière asynchrone
        Task.Run(async () => await _onnxService.InitializeAsync());

        InitializePopulation(500);
    }

    [RelayCommand]
    private void ResetSimulation()
    {
        InitializePopulation(500);
    }

    [RelayCommand]
    private async Task RunInferenceAsync()
    {
        // Exemple : Exécuter l'inférence sur tous les agents
        foreach (var agent in Population)
        {
            var prediction = _onnxService.Predict(agent.OpinionVector);

            // Mettre à jour l'agent selon la prédiction
            // Exemple : interpréter le résultat comme une nouvelle opinion
            agent.OpinionVector = prediction;

            // Mettre à jour la couleur en fonction du résultat
            agent.RenderColor = GetColorFromOpinion(prediction[0]);
        }
    }

    private SKColor GetColorFromOpinion(float opinionScore)
    {
        // Exemple : vert pour positif, rouge pour négatif
        if (opinionScore > 0.6f) return SKColors.Green;
        if (opinionScore < 0.4f) return SKColors.Red;
        return SKColors.Gray;
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
                OpinionVector = new float[5] // 5 dimensions selon votre modèle
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