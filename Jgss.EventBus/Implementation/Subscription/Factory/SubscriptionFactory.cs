using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal class SubscriptionFactory(ILoggerFactory loggerFactory) : ISubscriptionFactory
{
    public ISubscriptionImplementation CreateSubscription(string? subscriptionName, IEventRouter eventRouter) =>
        new Subscription(loggerFactory.CreateLogger<Subscription>(), subscriptionName, eventRouter);
}