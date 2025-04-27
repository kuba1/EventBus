using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Jgss.EventBus;

public class FirstBackgroundService : BackgroundService
{
    private const string ServiceName = nameof(FirstBackgroundService);

    private readonly ILogger logger;
    private readonly IBus bus;
    private readonly ISubscription subscription;

    public FirstBackgroundService(ILogger<FirstBackgroundService> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;

        logger.LogInformation("Creating {ServiceName}", ServiceName);

        subscription = bus.Subscribe(nameof(FirstBackgroundService));

        subscription
            .Synchronously("main")
            .Handle<SecondBackgroundServiceStarted>(HandleSecondBackgroundServiceStarted);
    }

    protected override async Task ExecuteAsync(CancellationToken gracefulShutdownToken)
    {
        logger.LogInformation("Starting {ServiceName}", ServiceName);

        await subscription.ProcessEventsAsync(gracefulShutdownToken);

        bus.Unsubscribe(subscription);

        logger.LogInformation("Exiting {ServiceName}", ServiceName);
    }

    private void HandleSecondBackgroundServiceStarted(SecondBackgroundServiceStarted secondBackgroundServiceStarted)
    {
        logger.LogInformation(
            "Second background service started and sent a message: {Message}",
            secondBackgroundServiceStarted.Message);
    }
}