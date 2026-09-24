using System.Windows;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
