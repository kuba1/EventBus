using Jgss.EventBus;

namespace Jgss.EventBus.Examples.Web.Events;

interface IEventWithId : IEvent
{
    Guid Id { get; }
}