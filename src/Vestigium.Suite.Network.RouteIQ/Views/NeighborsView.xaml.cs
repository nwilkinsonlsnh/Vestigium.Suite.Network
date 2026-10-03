using System.Windows;
using System.Windows.Controls;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class NeighborsView : UserControl
{
    public NeighborsView()
    {
        InitializeComponent();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => FitIpv4();

    private void FitIpv4()
    {
        var window = Window.GetWindow(this);
        var budget = window?.ActualHeight ?? ActualHeight;
        var used = Toolbar.ActualHeight + Ipv4Caption.ActualHeight + Ipv6Caption.ActualHeight + 276 + 210;
        var left = budget - used;
        Ipv4Host.Height = left > 160 ? left : 160;
        Ipv4Host.MaxHeight = Ipv4Host.Height;
    }
}
