using Jgss.EventBus.Examples.Web.Events;

namespace Jgss.EventBus.Examples.Web;

public interface IRequestEventPublisher
{
    Task<ResponseGenerated> PublishRequestEventAsync(RequestReceived request);
}