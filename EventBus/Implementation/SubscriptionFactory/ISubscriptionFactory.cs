namespace Jgss.EventBus.Implementation;

internal interface ISubscriptionFactory
{
    ISubscriptionImplementation CreateSubscription(string? subscriptionName, IEventRouter eventRouter);
}
