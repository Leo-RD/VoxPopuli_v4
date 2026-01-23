using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VoxPopuli.Client.Models;

namespace VoxPopuli.Client.ViewModels;

// CORRECTION 1 : Ajout du mot-clé 'partial' ici
public partial class SimulationViewModel : BaseViewModel
{
    // Liste simple pour les performances (pas d'ObservableCollection pour 500 agents)
    public List<AgentModel> Population { get; private set; } = new();

    // Le Toolkit va générer : public int AgentCount { get; set; }
    [ObservableProperty]
    private int agentCount;

    public SimulationViewModel()
    {
        InitializePopulation(500);
    }

    [RelayCommand]
    private void ResetSimulation()
    {
        InitializePopulation(500);
    }

    [RelayCommand]
    private void TriggerEvent()
    {
        // Exemple simple : changer la couleur de tout le monde
        foreach (var agent in Population)
        {
            agent.CurrentEmotion = EmotionalState.Agitated;
            agent.RenderColor = SKColors.Red;
        }
        // Force le rafraîchissement si nécessaire (souvent géré par la boucle de jeu)
    }

    private void InitializePopulation(int count)
    {
        var random = new Random();

        // On vide et on recrée
        Population.Clear();

        // Mise à jour de la propriété générée (nécessite 'partial' sur la classe)
        AgentCount = count;

        for (int i = 0; i < count; i++)
        {
            Population.Add(new AgentModel
            {
                // Position aléatoire (0 à 1000)
                X = random.Next(0, 1000),
                Y = random.Next(0, 1000),

                // CORRECTION 2 : Assignation d'un tableau float
                OpinionVector = new float[] { (float)random.NextDouble() },

                CurrentEmotion = EmotionalState.Neutral,
                RenderColor = SKColors.BlueViolet
            });
        }
    }
}