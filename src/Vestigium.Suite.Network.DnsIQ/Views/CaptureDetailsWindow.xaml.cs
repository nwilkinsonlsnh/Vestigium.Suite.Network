using System.Windows;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class CaptureDetailsWindow : Window
{
    public CaptureDetailsWindow()
    {
        InitializeComponent();
    }

    public void ShowHost(MainViewModel host, CaptureLine line, IReadOnlyList<CaptureLine> lines)
    {
        Title = "DnsIQ — " + line.Host;
        Details.Show(host, line, lines);
    }

    private void OnCloseRequested(object sender, EventArgs e) => Close();
}
