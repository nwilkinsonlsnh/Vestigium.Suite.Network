using System.Windows.Controls;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class DashboardView : UserControl
{
    private readonly PageViewport _viewport;

    public DashboardView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }
}
