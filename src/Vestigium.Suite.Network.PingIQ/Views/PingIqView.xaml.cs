using System.Windows.Controls;

namespace Vestigium.Suite.Network.PingIQ.Views;

public partial class PingIqView : UserControl
{
    private readonly PageViewport _viewport;

    public PingIqView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }
}
