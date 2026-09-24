using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Vestigium.Helpers.Kql;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ.Views;

internal sealed class QueryComplete
{
    private readonly TextBox _box;
    private readonly Popup _popup;
    private readonly ListBox _list;
    private readonly Func<MainViewModel, string, int, KqlCompletion> _complete;
    private readonly Func<MainViewModel, string, KqlCompletion, int, string> _apply;
    private readonly DispatcherTimer _timer;
    private KqlCompletion _last = KqlCompletion.Empty;
    private MainViewModel? _host;
    private bool _applying;
    private bool _suppress;

    public QueryComplete(
        TextBox box,
        Popup popup,
        ListBox list,
        Func<MainViewModel, string, int, KqlCompletion> complete,
        Func<MainViewModel, string, KqlCompletion, int, string> apply)
    {
        _box = box;
        _popup = popup;
        _list = list;
        _complete = complete;
        _apply = apply;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(140) };
        _timer.Tick += (_, _) => Show();
        if (box.TryFindResource("ListBox.QueryComplete") is Style style)
            list.Style = style;
        _list.MouseDoubleClick += (_, _) => Accept();
    }

    public void Place(string text)
    {
        _timer.Stop();
        _suppress = true;
        _applying = true;
        _popup.IsOpen = false;
        _box.Text = text;
        _box.CaretIndex = text.Length;
        _box.Focus();
        _box.Dispatcher.BeginInvoke(() =>
        {
            _box.CaretIndex = _box.Text.Length;
            _box.SelectionLength = 0;
            _popup.IsOpen = false;
            _applying = false;
            _suppress = false;
        }, DispatcherPriority.Input);
    }

    public void OnText(MainViewModel host)
    {
        if (_applying || _suppress)
        {
            _popup.IsOpen = false;
            return;
        }

        _host = host;
        _timer.Stop();
        _timer.Start();
    }

    public void OnKey(MainViewModel host, KeyEventArgs e)
    {
        if (!_popup.IsOpen)
            return;

        if (e.Key == Key.Escape)
        {
            _popup.IsOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            _list.SelectedIndex = Math.Min(_list.Items.Count - 1, _list.SelectedIndex + 1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            _list.SelectedIndex = Math.Max(0, _list.SelectedIndex - 1);
            e.Handled = true;
            return;
        }

        if (e.Key != Key.Tab)
            return;

        Accept(host);
        e.Handled = true;
    }

    private void Show()
    {
        _timer.Stop();
        if (_host is null || _applying || _suppress)
            return;

        _last = _complete(_host, _box.Text, _box.CaretIndex);
        _list.ItemsSource = _last.Rows;
        if (_last.Rows.Count == 0)
        {
            _popup.IsOpen = false;
            return;
        }

        _list.SelectedIndex = 0;
        var rect = _box.GetRectFromCharacterIndex(_box.CaretIndex);
        _popup.Placement = PlacementMode.Relative;
        _popup.PlacementTarget = _box;
        _popup.HorizontalOffset = rect.X;
        _popup.VerticalOffset = rect.Bottom;
        _popup.IsOpen = true;
    }

    private void Accept()
    {
        if (_box.DataContext is MainViewModel host)
            Accept(host);
    }

    private void Accept(MainViewModel host)
    {
        var index = _list.SelectedIndex < 0 ? 0 : _list.SelectedIndex;
        if (index >= _last.Rows.Count)
            return;

        var next = _apply(host, _box.Text, _last, index);
        if (!next.EndsWith('(') && !next.EndsWith(' '))
            next += " ";

        _timer.Stop();
        _applying = true;
        _popup.IsOpen = false;
        _box.Text = next;
        _box.CaretIndex = next.Length;
        _box.Focus();
        _box.Dispatcher.BeginInvoke(() =>
        {
            _box.CaretIndex = _box.Text.Length;
            _box.SelectionLength = 0;
            _applying = false;
            if (_box.DataContext is MainViewModel current)
                OnText(current);
        }, DispatcherPriority.Input);
    }
}
