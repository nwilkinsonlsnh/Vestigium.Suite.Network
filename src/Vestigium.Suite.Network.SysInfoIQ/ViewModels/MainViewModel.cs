using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public SettingsViewModel? Settings { get; set; }

    [ObservableProperty]
    private string _summary = "";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<InventorySection> Sections { get; } = new();

    [RelayCommand(CanExecute = nameof(CanCollect))]
    private async Task CollectAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        CollectCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        Sections.Clear();
        Summary = "";

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        try
        {
            Status("Collecting base…");
            var sections = await Task.Run(() => Collector.CollectBase(ct), ct).ConfigureAwait(true);
            foreach (var s in sections)
                Sections.Add(s);

            var location = Settings?.OutputFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var now = DateTime.Now;
            Summary = Collector.BuildSummary(Sections, location, now);
            Status("Idle");
        }
        catch (OperationCanceledException)
        {
            Status("Cancelled");
        }
        catch (Exception ex)
        {
            Summary = ex.Message;
            Status("Failed");
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
            CollectCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private bool CanCollect() => !IsBusy;

    private void Status(string message)
    {
        if (StatusBar is not null)
            StatusBar.Message = message;
    }
}
