using Vestigium.Helpers.Kql;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    public KqlCompletion CompleteRoute(string text, int caret)
        => KqlHelper.Complete(text, caret, _routeSession, Hints(RouteIqSession.RouteTab));

    public KqlCompletion CompleteNeighbor(string text, int caret)
        => KqlHelper.Complete(text, caret, _neighborSession, Hints(RouteIqSession.NeighborTab));

    public KqlCompletion CompleteConnection(string text, int caret)
        => KqlHelper.Complete(text, caret, _connectionSession, Hints(RouteIqSession.ConnectionTab));

    public string ApplyRoute(string text, KqlCompletion completion, int index)
        => Apply(text, completion, index, value => RouteQuery = value);

    public string ApplyNeighbor(string text, KqlCompletion completion, int index)
        => Apply(text, completion, index, value => NeighborQuery = value);

    public string ApplyConnection(string text, KqlCompletion completion, int index)
        => Apply(text, completion, index, value => ConnectionQuery = value);

    private string[] Hints(string tab)
        => QueryBook?.Queries.Where(entry => entry.Tab == tab).Select(entry => entry.Text).ToArray() ?? [];

    private static string Apply(string text, KqlCompletion completion, int index, Action<string> store)
    {
        if (index < 0 || index >= completion.Rows.Count)
            return text;
        var start = Math.Clamp(completion.ReplaceStart, 0, text.Length);
        var length = Math.Clamp(completion.ReplaceLength, 0, text.Length - start);
        var next = text.Remove(start, length).Insert(start, completion.Rows[index].Insert);
        store(next);
        return next;
    }
}
