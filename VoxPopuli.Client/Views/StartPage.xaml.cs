using VoxPopuli.Client.ViewModels;

namespace VoxPopuli.Client.Views;

public partial class StartPage : ContentPage
{
    private readonly StartPageViewModel _viewModel;

    public StartPage(StartPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnPresetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.Text, out int count))
        {
            _viewModel.SelectedAgentCount = count;
        }
    }

    private void OnSplitPresetClicked(object sender, EventArgs e)
    {
        if (sender is not Button button) return;

        _viewModel.LeftPercentage = button.Text switch
        {
            "100G" => 100,
            "70G"  => 70,
            "50/50" => 50,
            "70D"  => 30,
            "100D" => 0,
            _ => 50
        };
    }
}
