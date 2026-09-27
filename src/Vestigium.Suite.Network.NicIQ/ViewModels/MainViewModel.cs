using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Adapters = [];
    }

    public ObservableCollection<AdapterRow> Adapters { get; }

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    [ObservableProperty]
    private string _header = "NicIQ";

    [ObservableProperty]
    private string _caption = "Idle";

    [ObservableProperty]
    private string _detail = string.Empty;

    [ObservableProperty]
    private bool _includeDown = true;

    [ObservableProperty]
    private decimal _durationSeconds = 10m;

    [ObservableProperty]
    private AdapterRow? _selectedAdapter;

    partial void OnSelectedAdapterChanged(AdapterRow? value)
        => Detail = value?.Detail ?? string.Empty;

    partial void OnIncludeDownChanged(bool value) => Refresh();

    [RelayCommand]
    private void Refresh()
    {
        var keep = SelectedAdapter?.Id;
        try
        {
            var box = NetworkHelper.GetWorkstation();
            Header = FormatHeader(box);
            var query = new NetworkAdapterQuery(IncludeDown: IncludeDown);
            var rows = NetworkHelper.GetAdapters(query).Select(static a => new AdapterRow(a)).ToList();
            ReplaceRows(rows, keep);
            Post("Idle");
        }
        catch (Exception ex)
        {
            Post("Failed", ex.Message);
        }
    }

    private void ReplaceRows(IReadOnlyList<AdapterRow> rows, string? keepId)
    {
        Adapters.Clear();
        foreach (var row in rows)
            Adapters.Add(row);

        if (!string.IsNullOrWhiteSpace(keepId))
        {
            var match = Adapters.FirstOrDefault(r => string.Equals(r.Id, keepId, StringComparison.Ordinal));
            SelectedAdapter = match;
            return;
        }

        if (SelectedAdapter is not null && Adapters.Contains(SelectedAdapter))
            return;

        SelectedAdapter = Adapters.Count > 0 ? Adapters[0] : null;
    }

    private void Post(string status, string? detail = null)
    {
        Caption = string.IsNullOrWhiteSpace(detail) ? status : $"{status}  {detail}";
        if (StatusBar is null)
            return;

        StatusBar.Message = Caption;
    }

    private static string FormatHeader(WorkstationNetwork box)
    {
        var host = string.IsNullOrWhiteSpace(box.HostName) ? "—" : box.HostName.Trim();
        if (string.IsNullOrWhiteSpace(box.DomainName))
            return host;
        return $"{host}  /  {box.DomainName.Trim()}";
    }
}
