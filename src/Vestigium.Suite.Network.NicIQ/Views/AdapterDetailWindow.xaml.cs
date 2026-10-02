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
        var row = viewModel.SelectedAdapter ?? viewModel.SelectedMonitorNic;
        if (row is null)
            return;

        var form = AdapterDetailForm.From(row);
        if (_open is { IsLoaded: true })
        {
            _open.DataContext = form;
            _open.Activate();
            return;
        }

        var window = new AdapterDetailWindow
        {
            DataContext = form,
            Owner = owner
        };
        window.Closed += (_, _) => _open = null;
        if (owner is not null)
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _open = window;
        window.Show();
    }

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdapterDetailForm form)
            Clipboard.SetText(form.CopyText);
    }
}
