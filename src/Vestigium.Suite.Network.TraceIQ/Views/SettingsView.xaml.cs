using System.Windows.Controls;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void ListEdited(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            Dispatcher.BeginInvoke(vm.CommitList);
    }
}
