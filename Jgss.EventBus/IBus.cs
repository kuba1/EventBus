namespace Jgss.EventBus;

/// <summary>
/// Event bus
/// </summary>
public interface IBus
{
    ISubscription Subscribe(string? subscriptionName = null);
    void Unsubscribe(ISubscription subscription);
}