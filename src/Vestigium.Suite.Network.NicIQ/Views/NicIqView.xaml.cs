using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vestigium.Suite.Network.NicIQ.ViewModels;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class NicIqView : UserControl
{
    private readonly PageViewport _viewport;

    public NicIqView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }

    private void AdapterGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;
        AdapterDetailWindow.ShowFor(Window.GetWindow(this), vm);
    }
}
