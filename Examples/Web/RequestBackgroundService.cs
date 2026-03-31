using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Jgss.EventBus;

public sealed class RequestBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(RequestBackgroundService);

    private readonly ILogger logger;
    private readonly IBus bus;
    private readonly ISubscription subscription;

    public RequestBackgroundService(ILogger<RequestBackgroundService> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(nameof(RequestBackgroundService));

        subscription
            .Asynchronously()
            .Handle<RequestReceived>(HandleRequestReceivedAsync);
    }

    protected override async Task ExecuteAsync(CancellationToken gracefulShutdownToken)
    {
        logger.LogInformation("Starting {ServiceName}", ServiceName);

        await subscription.ProcessEventsAsync(gracefulShutdownToken);

        bus.Unsubscribe(subscription);

        logger.LogInformation("Exiting {ServiceName}", ServiceName);
    }

    private async Task HandleRequestReceivedAsync(RequestReceived requestReceived)
    {
        logger.LogInformation(
            "Received a request: {Message}",
            requestReceived.Message);

        await Task.Delay(1000); // Simulated I/O bound operation that takes some time to execute

        subscription.Publish(new ResponseGenerated { Message = "Response has been received" });

        logger.LogInformation("Response sent");
    }
}