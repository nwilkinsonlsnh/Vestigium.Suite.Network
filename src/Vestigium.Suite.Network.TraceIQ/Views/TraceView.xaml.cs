using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class TraceView : UserControl
{
    public TraceView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) => Rebuild();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyPropertyChanged oldVm)
            oldVm.PropertyChanged -= OnViewModelChanged;
        if (e.NewValue is INotifyPropertyChanged vm)
            vm.PropertyChanged += OnViewModelChanged;
        Rebuild();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.ProbeColumns))
            Rebuild();
    }

    private void Rebuild()
    {
        if (DataContext is not MainViewModel vm)
            return;

        var count = Math.Clamp(vm.ProbeColumns, 5 > 0 ? 1 : 1, 5);
        while (HopGrid.Columns.Count > 4)
            HopGrid.Columns.RemoveAt(HopGrid.Columns.Count - 1);

        for (var i = 1; i <= count; i++)
        {
            HopGrid.Columns.Add(new DataGridTextColumn
            {
                Header = $"P{i} (ms)",
                Binding = new Binding($"P{i}"),
                Width = 90
            });
        }
    }
}
