using System.Windows;
using Vestigium.Suite.Network.ProbeHost.ViewModels;

namespace Vestigium.Suite.Network.ProbeHost;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
