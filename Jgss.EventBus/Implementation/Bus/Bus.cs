using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Jgss.EventBus.Implementation;

internal class Bus(ILogger<Bus> logger, ISubscriptionFactory subscriptionFactory) : IBusImplementation
{
    private readonly ConcurrentDictionary<Guid, ISubscriptionImplementation> subscriptions = new();

    public ISubscription Subscribe(string? subscriptionName = null)
    {
        var subscription = subscriptionFactory.CreateSubscription(subscriptionName, this);

        subscriptions.TryAdd(subscription.Id, subscription);

        logger.LogDebug("Subscription {SubscriptionName} has subscribed", subscription.Name);

        return subscription;
    }

    public void Unsubscribe(ISubscription subscription)
    {
        var existed = subscriptions.TryRemove(subscription.Id, out _);

        if (existed)
            logger.LogDebug("Subscription {SubscriptionName} has unsubscribed", subscription.Name);
        else
            logger.LogWarning("Subscription {SubscriptionName} is already unsubscribed", subscription.Name);
    }

    public void Publish(IEvent publishedEvent)
    {
        var targetSubscriptions = publishedEvent.GetType().GetCustomAttribute<TargetSubscriptionsAttribute>();

        foreach (var subscription in subscriptions.Values)
        {
            if (targetSubscriptions is null || targetSubscriptions.Contains(subscription.Name))
                subscription.Receive(publishedEvent);
        }

        logger.LogDebug("Publishing {EventTypeName} event", publishedEvent.GetType().Name);
    }
}