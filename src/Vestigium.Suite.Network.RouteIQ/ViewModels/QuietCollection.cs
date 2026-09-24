using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed class QuietCollection<T> : ObservableCollection<T>
{
    private bool _quiet;

    public void Reset(IReadOnlyList<T> items)
    {
        _quiet = true;
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
        _quiet = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_quiet)
            base.OnCollectionChanged(e);
    }
}
