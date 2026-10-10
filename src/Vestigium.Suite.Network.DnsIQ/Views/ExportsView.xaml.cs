using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class ExportsView : UserControl
{
    private bool _loading;

    public ExportsView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel host)
            return;
        _loading = true;
        LookupBox.IsChecked = host.ExportLookup;
        CaptureBox.IsChecked = host.ExportCapture;
        ProbeBox.IsChecked = host.ExportProbe;
        MonitoringBox.IsChecked = host.ExportMonitoring;
        OpenBox.IsChecked = host.ExportOpenAfter;
        OpenFolderBox.IsChecked = host.ExportOpenFolder;
        _loading = false;
        host.NotifyExport();
    }

    private void OnCheck(object sender, RoutedEventArgs e)
    {
        if (_loading || DataContext is not MainViewModel host)
            return;
        host.RememberExportChecks(
            LookupBox.IsChecked == true,
            CaptureBox.IsChecked == true,
            ProbeBox.IsChecked == true,
            MonitoringBox.IsChecked == true,
            OpenBox.IsChecked == true,
            OpenFolderBox.IsChecked == true);
    }
}
