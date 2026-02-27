using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VoxPopuli.Client.ViewModels;

public partial class StartPageViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedAgentCount = 500;

    [ObservableProperty]
    private string agentCountDisplay = "500 agents";

    /// <summary>
    /// Met à jour l'affichage du nombre d'agents
    /// </summary>
    partial void OnSelectedAgentCountChanged(int value)
    {
        AgentCountDisplay = $"{value} agents";
    }

    /// <summary>
    /// Lance la simulation avec le nombre d'agents choisi
    /// </summary>
    [RelayCommand]
    private async Task StartSimulation()
    {
        var parameters = new Dictionary<string, object>
        {
            { "AgentCount", SelectedAgentCount }
        };

        await Shell.Current.GoToAsync("//SimulationPage", parameters);
    }
}
