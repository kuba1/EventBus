namespace Jgss.EventBus;

/// <summary>
/// Event bus
/// </summary>
public interface IEventBus
{
    ISubscription Subscribe(string? subscriptionName = null);
}