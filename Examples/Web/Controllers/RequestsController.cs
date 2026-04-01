using Microsoft.AspNetCore.Mvc;

using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestsController(ILogger<RequestsController> logger, IRequestEventPublisher eventPublisher) : ControllerBase
{
    [HttpGet]
    public async Task<string> Get()
    {
        var response = await eventPublisher.PublishRequestEventAsync(new RequestReceived { Message = "Request to get data" });

        logger.LogInformation("Reponse has been received with message: {ResponseMessage}", response.Message);

        return response.Message;
    }
}
