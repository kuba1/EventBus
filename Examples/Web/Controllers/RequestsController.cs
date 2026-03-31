using Microsoft.AspNetCore.Mvc;

using Jgss.EventBus;

namespace Web.Controllers;

class RequestResponseHandler(ILogger logger, IBus bus)
{
    public async Task<string> SendRequestAndGetResponseAsync(RequestReceived request)
    {
        using var cancellation = new CancellationTokenSource();

        var subscription = bus.Subscribe();

        var response = string.Empty;

        try
        {
            subscription
                .Synchronously()
                .Handle<ResponseGenerated>((ResponseGenerated responseEvent) =>
                {
                    logger.LogInformation("Response received, setting result");

                    response = responseEvent.Message;

                    cancellation.Cancel();
                });

            subscription.Publish(request);

            logger.LogInformation("Request sent");

            return await Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation("Waiting for response");

                    await subscription.ProcessEventsAsync(cancellation.Token);
                }
                catch
                {
                }

                logger.LogInformation("Finished waiting for response, returning");

                return response;
            });
        }
        finally
        {
            bus.Unsubscribe(subscription);
        }
    }
}

[ApiController]
[Route("[controller]")]
public class RequestsController(ILogger<RequestsController> logger, IBus bus) : ControllerBase
{
    [HttpGet]
    public async Task<string> Get()
    {
        var handler = new RequestResponseHandler(logger, bus);

        var responseMessage = await handler.SendRequestAndGetResponseAsync(new RequestReceived { Message = "Request to get data" });

        logger.LogInformation("Reponse has been recevied with message: {ResponseMessage}", responseMessage);

        return responseMessage;
    }
}
