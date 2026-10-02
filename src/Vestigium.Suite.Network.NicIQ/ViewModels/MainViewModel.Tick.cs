namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private SampleTick TakeTickSafe(string name, string description, IReadOnlyList<string> selected, CancellationToken token)
    {
        try
        {
            return TakeTick(name, description, selected, token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SampleTick(null, null, ex.Message, false);
        }
    }
}
