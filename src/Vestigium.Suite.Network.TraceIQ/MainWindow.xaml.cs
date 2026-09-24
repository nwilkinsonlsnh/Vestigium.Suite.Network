using System.Windows;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigigium.Suite.Network.TraceIQ;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
