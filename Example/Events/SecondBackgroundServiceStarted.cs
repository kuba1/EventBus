using Jgss.EventBus;

public class SecondBackgroundServiceStarted : IEvent
{
    public string Message { get; init; } = string.Empty;
}