using System.Windows;
using Vestigium.Suite.Network.PingIQ.ViewModels;

namespace Vestigium.Suite.Network.PingIQ;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
