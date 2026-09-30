namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private async Task RunMonitorLoopAsync(CancellationToken token)
    {
        await Task.Yield();
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (MonitorPaused)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(250), token).ConfigureAwait(true);
                    continue;
                }

                var nic = SelectedMonitorNic;
                if (nic is null)
                {
                    MonitorInstance = string.Empty;
                    Note("Idle", "No active NIC");
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(true);
                    continue;
                }

                var name = nic.Name;
                var description = nic.Source.Description;
                var selected = Settings is null
                    ? MonitorCounterList.FromSettings(Session?.Current ?? new NicIqSettings())
                    : MonitorCounterList.Sanitize(Settings.MonitorCounters);

                var tick = await Task.Run(() => TakeTick(name, description, selected, token), token).ConfigureAwait(true);
                if (!string.Equals(MonitorInstance, tick.Instance, StringComparison.Ordinal))
                    MonitorInstance = tick.Instance ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(tick.Error))
                {
                    Note("Failed", tick.Error);
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(true);
                    continue;
                }

                if (tick.Primed)
                {
                    Note($"Monitoring {name}", tick.Instance);
                    await Task.Delay(TimeSpan.FromMilliseconds(250), token).ConfigureAwait(true);
                    continue;
                }

                if (tick.Result is not null)
                    ApplySamples(tick.Result);
                Note($"Monitoring {name}", tick.Instance);
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Note("Failed", ex.Message);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
