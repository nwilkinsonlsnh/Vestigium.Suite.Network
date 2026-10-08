using System.ComponentModel;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed class DetailRow : INotifyPropertyChanged
{
    private string _check;
    private string _question;

    public DetailRow(string category, string answer)
    {
        Category = category;
        Answer = answer;
        _check = ReverseName.TryPtr(answer, out var question) ? "Pending" : "";
        _question = question;
    }

    public string Category { get; }

    public string Answer { get; }

    public string Check
    {
        get => _check;
        set
        {
            if (_check == value)
                return;
            _check = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Check)));
        }
    }

    public string Question
    {
        get => _question;
        set
        {
            if (_question == value)
                return;
            _question = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Question)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
