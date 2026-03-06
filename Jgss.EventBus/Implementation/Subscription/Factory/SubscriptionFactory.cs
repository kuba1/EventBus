using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal sealed class SubscriptionFactory(ILoggerFactory loggerFactory) : ISubscriptionFactory
{
    public ISubscriptionImplementation CreateSubscription(string? subscriptionName, IEventPublisher eventPublisher) =>
        new Subscription(loggerFactory.CreateLogger<Subscription>(), subscriptionName, eventPublisher);
}