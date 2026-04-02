using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Jgss.EventBus;
using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web;

public sealed class ResponseBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(ResponseBackgroundService);

    private readonly ILogger logger;
    private readonly IBus bus;
    private readonly ISubscription subscription;

    public ResponseBackgroundService(ILogger<ResponseBackgroundService> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(nameof(ResponseBackgroundService));

        subscription
            .Asynchronously()
            .Handle<IntermediateEvent>(HandleIntermediateEventAsync);
    }

    protected override async Task ExecuteAsync(CancellationToken gracefulShutdownToken)
    {
        logger.LogInformation("Starting {ServiceName}", ServiceName);

        await subscription.ProcessEventsAsync(gracefulShutdownToken);

        bus.Unsubscribe(subscription);

        logger.LogInformation("Exiting {ServiceName}", ServiceName);
    }

    private async Task HandleIntermediateEventAsync(IntermediateEvent intermediateEvent)
    {
        logger.LogInformation(
            "Received an intermediate event: {Message}",
            intermediateEvent.Message);

        await Task.Delay(1000); // Simulate I/O bound operation that takes some time to finish

        subscription.Publish(new ResponseGenerated(intermediateEvent) { Message = "Response has been received" });

        logger.LogInformation("Response sent");
    }
}
