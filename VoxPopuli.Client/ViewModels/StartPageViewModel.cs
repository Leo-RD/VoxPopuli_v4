using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VoxPopuli.Client.ViewModels;

public partial class StartPageViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedAgentCount = 500;

    [ObservableProperty]
    private string agentCountDisplay = "500 agents";

    // Pourcentage d'agents de gauche (0-100), droite = 100 - LeftPercentage
    [ObservableProperty]
    private int leftPercentage = 50;

    [ObservableProperty]
    private string leftCountDisplay = "250 gauche";

    [ObservableProperty]
    private string rightCountDisplay = "250 droite";

    [ObservableProperty]
    private string splitDisplay = "50% ◀ Gauche  |  Droite ▶ 50%";

    partial void OnSelectedAgentCountChanged(int value)
    {
        AgentCountDisplay = $"{value} agents";
        UpdateSplitDisplay(value, LeftPercentage);
    }

    partial void OnLeftPercentageChanged(int value)
    {
        UpdateSplitDisplay(SelectedAgentCount, value);
    }

    private void UpdateSplitDisplay(int total, int leftPct)
    {
        int leftCount = (int)Math.Round(total * leftPct / 100.0);
        int rightCount = total - leftCount;
        int rightPct = 100 - leftPct;

        LeftCountDisplay = $"{leftCount}";
        RightCountDisplay = $"{rightCount}";
        SplitDisplay = $"{leftPct}% Gauche  |  Droite {rightPct}%";
    }

    [RelayCommand]
    private async Task StartSimulation()
    {
        var parameters = new Dictionary<string, object>
        {
            { "AgentCount", SelectedAgentCount },
            { "LeftPercentage", LeftPercentage }
        };

        await Shell.Current.GoToAsync("//SimulationPage", parameters);
    }
}
