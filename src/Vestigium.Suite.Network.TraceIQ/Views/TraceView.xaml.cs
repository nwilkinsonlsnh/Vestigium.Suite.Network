using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class TraceView : UserControl
{
    public TraceView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) =>
        {
            HoldTheForm();
            Rebuild();
        };
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

    private void HoldTheForm()
    {
        var scroller = FindAncestor<ScrollViewer>(this);
        if (scroller is null)
            return;
        scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        var current = start;
        while (current is not null)
        {
            if (current is T found)
                return found;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void Rebuild()
    {
        if (DataContext is not MainViewModel vm)
            return;

        var count = Math.Clamp(vm.ProbeColumns, 5 > 0 ? 1 : 1, 5);
        while (HopGrid.Columns.Count > 3)
            HopGrid.Columns.RemoveAt(HopGrid.Columns.Count - 1);

        for (var i = 1; i <= count; i++)
        {
            HopGrid.Columns.Add(new DataGridTextColumn
            {
                Header = $"RTT {i} (ms)",
                Binding = new Binding($"P{i}"),
                Width = 90
            });
        }
    }
}
