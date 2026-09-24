using System.Windows;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
