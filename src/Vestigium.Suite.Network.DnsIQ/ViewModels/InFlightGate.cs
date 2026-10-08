namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class InFlightGate
{
    public static async Task WaitAsync<TTask>(List<TTask> inflight, int cap, CancellationToken token)
        where TTask : Task
    {
        while (inflight.Count >= cap)
        {
            token.ThrowIfCancellationRequested();
            var done = await Task.WhenAny(inflight).WaitAsync(token).ConfigureAwait(false);
            inflight.Remove((TTask)done);
        }
    }
}
