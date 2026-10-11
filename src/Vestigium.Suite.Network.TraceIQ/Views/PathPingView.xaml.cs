using System.ComponentModel;
using System.Windows.Controls;
using Vestigium.Suite.Network.TraceIQ.ViewModels;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class PathPingView : UserControl
{
    public PathPingView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Watch();
        Loaded += (_, _) => Watch();
    }

    private void Watch()
    {
        if (DataContext is not PathPingViewModel vm)
            return;
        vm.Rows.CollectionChanged -= RowsChanged;
        vm.Rows.CollectionChanged += RowsChanged;
        foreach (var row in vm.Rows)
            Hook(row);
    }

    private void RowsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
            return;
        foreach (PathPingRow row in e.NewItems)
            Hook(row);
    }

    private void Hook(PathPingRow row)
    {
        row.PropertyChanged -= RowChanged;
        row.PropertyChanged += RowChanged;
    }

    private void RowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PathPingRow.Lossy) or nameof(PathPingRow.Loss))
            Mark(sender as PathPingRow);
    }

    private void MarkRow(object sender, DataGridRowEventArgs e) => Mark(e.Row.Item as PathPingRow);

    private void Mark(PathPingRow? item)
    {
        if (item is null || DataContext is not PathPingViewModel vm)
            return;
        var row = Grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        if (row is null)
            return;
        if (item.Lossy)
        {
            row.Background = vm.LossFillBrush;
            row.Foreground = vm.LossInkBrush;
        }
        else
        {
            row.ClearValue(DataGridRow.BackgroundProperty);
            row.ClearValue(DataGridRow.ForegroundProperty);
        }
    }
}
