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
}
