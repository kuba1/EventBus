using Jgss.EventBus;

[TargetSubscriptions(nameof(FirstBackgroundService))]
public class SecondBackgroundServiceStarted : IEvent
{
    public string Message { get; init; } = string.Empty;
}