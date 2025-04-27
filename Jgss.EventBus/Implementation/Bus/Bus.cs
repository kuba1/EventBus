using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal class Bus(ILogger<Bus> logger, ISubscriptionFactory subscriptionFactory) : IBusImplementation
{
    private readonly ConcurrentDictionary<Guid, ISubscriptionImplementation> subscriptions = new();

    public ISubscription Subscribe(string? subscriptionName = null)
    {
        var subscription = subscriptionFactory.CreateSubscription(subscriptionName, this);

        subscriptions.TryAdd(subscription.Id, subscription);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Subscription {SubscriptionName} has subscribed", subscription.Name);

        return subscription;
    }

    public void Unsubscribe(ISubscription subscription)
    {
        var existed = subscriptions.TryRemove(subscription.Id, out _);

        if (existed && logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Subscription {SubscriptionName} has unsubscribed", subscription.Name);

        if (!existed && logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("Subscription {SubscriptionName} is already unsubscribed", subscription.Name);
    }

    public void Publish(IEvent publishedEvent)
    {
        foreach (var subscription in subscriptions.Values)
            subscription.Receive(publishedEvent);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Publishing {EventTypeName} event", publishedEvent.GetType().Name);
    }
}