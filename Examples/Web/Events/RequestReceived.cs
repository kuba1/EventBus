using Jgss.EventBus;

public class RequestReceived : IEvent
{
    public string Message { get; init; } = string.Empty;
}
