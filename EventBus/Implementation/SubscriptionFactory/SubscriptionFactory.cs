namespace Jgss.EventBus.Implementation;

internal class SubscriptionFactory : ISubscriptionFactory
{
    public ISubscriptionImplementation CreateSubscription(string? subscriptionName, IEventRouter eventRouter) =>
        new Subscription(subscriptionName, eventRouter);
}