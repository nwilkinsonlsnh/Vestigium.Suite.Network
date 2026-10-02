using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vestigium.Suite.Network.NicIQ.ViewModels;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class NicIqView : UserControl
{
    private readonly PageViewport _viewport;
    private AdapterDetailWindow? _detail;

    public NicIqView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }

    private void AdapterGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel { SelectedAdapter: not null } vm)
            return;

        if (_detail is { IsLoaded: true })
        {
            _detail.Activate();
            return;
        }

        var owner = Window.GetWindow(this);
        _detail = new AdapterDetailWindow
        {
            DataContext = vm,
            Owner = owner
        };
        _detail.Closed += (_, _) => _detail = null;
        if (owner is not null)
            _detail.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _detail.Show();
    }
}
