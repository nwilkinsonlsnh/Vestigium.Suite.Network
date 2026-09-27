namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed record NicIqWatchQuery(string AdapterKey, TimeSpan Duration);

public static class NicIqWatchInput
{
    public const int MinDurationSeconds = 1;
    public const int MaxDurationSeconds = 60;
    public const int DefaultDurationSeconds = 10;

    public static bool TryCreate(string? adapterKey, decimal durationSeconds, out NicIqWatchQuery? query, out string? reject)
    {
        query = null;
        reject = null;

        var key = adapterKey?.Trim() ?? string.Empty;
        if (key.Length == 0)
        {
            reject = "No adapter selected.";
            return false;
        }

        if (durationSeconds < MinDurationSeconds || durationSeconds > MaxDurationSeconds)
        {
            reject = "Duration must be 1–60 seconds.";
            return false;
        }

        query = new NicIqWatchQuery(key, TimeSpan.FromSeconds((int)durationSeconds));
        return true;
    }
}
