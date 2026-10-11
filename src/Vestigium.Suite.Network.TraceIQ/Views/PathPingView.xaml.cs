using System.Windows.Controls;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class PathPingView : UserControl
{
    public PathPingView() => InitializeComponent();

    private void MarkRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is not PathPingRow item || DataContext is not PathPingViewModel vm || !item.Lossy)
            return;
        e.Row.Background = vm.LossFillBrush;
        e.Row.Foreground = vm.LossInkBrush;
    }
}
