using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels;

namespace Vestigium.Suite.Network.SysInfoIQ.Views;

public partial class SysInfoView : UserControl
{
    public SysInfoView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && !vm.IsBusy && vm.SystemRows.Count == 0)
            vm.CollectCommand.Execute(null);
    }
}
