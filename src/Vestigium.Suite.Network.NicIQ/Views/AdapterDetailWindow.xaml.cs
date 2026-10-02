using System.Windows;
using Vestigium.Suite.Network.NicIQ.ViewModels;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class AdapterDetailWindow : Window
{
    private static AdapterDetailWindow? _open;

    public AdapterDetailWindow()
    {
        InitializeComponent();
    }

    public static void ShowFor(Window? owner, MainViewModel viewModel)
    {
        if (viewModel.SelectedAdapter is null && viewModel.SelectedMonitorNic is null)
            return;

        if (_open is { IsLoaded: true })
        {
            _open.DataContext = viewModel;
            _open.Activate();
            return;
        }

        var window = new AdapterDetailWindow
        {
            DataContext = viewModel,
            Owner = owner
        };
        window.Closed += (_, _) => _open = null;
        if (owner is not null)
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _open = window;
        window.Show();
    }
}
