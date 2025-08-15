namespace Jgss.EventBus;

/// <summary>
/// If used on an event, the event will only be received by subscriptions
/// with given names.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TargetSubscriptionsAttribute(params string[] targetSubscriptionsNames) : Attribute
{
    public bool Contains(string subscriptionName) => targetSubscriptionsNames.Contains(subscriptionName);
}
