using System.Windows;
using System.Windows.Controls;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class RoutesView : UserControl
{
    public RoutesView()
    {
        InitializeComponent();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => FitIpv4();

    private void FitIpv4()
    {
        var used = Toolbar.ActualHeight + Ipv4Caption.ActualHeight + Ipv6Caption.ActualHeight + 276 + 48;
        var left = ActualHeight - used;
        Ipv4Host.Height = left > 120 ? left : 120;
        Ipv4Host.MaxHeight = Ipv4Host.Height;
    }
}
