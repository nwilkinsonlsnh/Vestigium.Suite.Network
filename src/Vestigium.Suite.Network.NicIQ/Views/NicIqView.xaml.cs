using System.Windows.Controls;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class NicIqView : UserControl
{
    private readonly PageViewport _viewport;

    public NicIqView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }
}
