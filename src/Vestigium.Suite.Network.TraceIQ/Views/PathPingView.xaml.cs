using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class PathPingView : UserControl
{
    public PathPingView() => InitializeComponent();

    private void MarkRow(object sender, RoutedEventArgs e)
    {
        if (sender is not DataGridRow row || row.Item is not ViewModels.PathPingRow item || DataContext is not ViewModels.PathPingViewModel vm)
            return;
        if (item.Lossy)
        {
            row.Background = vm.LossFillBrush;
            row.Foreground = vm.LossInkBrush;
        }
    }
}
