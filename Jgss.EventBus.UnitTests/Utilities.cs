namespace Jgss.EventBus.UnitTests;

internal static class Utilities
{
    public static async Task WaitUntilSubscriptionReceivedEvent(
        this Mock<ISubscriptionImplementation> subscriptionMock,
        IEvent eventToReceive,
        CancellationToken cancellationToken) =>
    await Task.Run(async () =>
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                subscriptionMock.Verify(s => s.Receive(eventToReceive), Times.Once);

                return;
            }
            catch
            {
            }

            await Task.Delay(100, cancellationToken);
        }
    });
}