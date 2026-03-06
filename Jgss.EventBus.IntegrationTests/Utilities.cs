namespace Jgss.EventBus.IntegrationTests;

internal class Utilities
{
    public static async Task WaitUntilSetAsync(CancellationToken cancellationToken, params ManualResetEventSlim[] eventsToSet) =>
        await Task.WhenAll(eventsToSet.Select(e => Task.Run(() => e.Wait(cancellationToken))));
}