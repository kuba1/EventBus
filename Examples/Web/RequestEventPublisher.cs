using System.Collections.Concurrent;

using Jgss.EventBus;
using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web;

class RequestEventPublisher : IRequestEventPublisher, IDisposable
{
    private readonly ILogger logger;
    private readonly IBus bus;

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseGenerated>> responses = new();

    private readonly ISubscription subscription;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Task eventHandlingTask;

    public RequestEventPublisher(ILogger<RequestEventPublisher> logger, IBus bus)
    {
        this.logger = logger;
        this.bus = bus;

        subscription = bus.Subscribe();

        subscription
            .Synchronously()
            .Handle<ResponseGenerated>((ResponseGenerated response) =>
            {
                logger.LogInformation($"Response with Id = \"{response.Id}\" received");

                if (!responses.TryRemove(response.Id, out var responseTask))
                    return;

                responseTask.SetResult(response);
            });

        eventHandlingTask = Task.Factory.StartNew(
            async () => await subscription.ProcessEventsAsync(cancellation.Token),
            TaskCreationOptions.LongRunning);
    }

    public async Task<ResponseGenerated> PublishRequestEventAsync(RequestReceived request)
    {
        var responseTaskCompletionSource = new TaskCompletionSource<ResponseGenerated>();

        if (!responses.TryAdd(request.Id, responseTaskCompletionSource))
            throw new InvalidOperationException($"Request with Id = \"{request.Id}\" is already pending");

        subscription.Publish(request);

        logger.LogInformation($"Request with Id = \"{request.Id}\" sent");

        return await responseTaskCompletionSource.Task;
    }

    public void Dispose()
    {
        bus.Unsubscribe(subscription);

        cancellation.Dispose();
    }
}
