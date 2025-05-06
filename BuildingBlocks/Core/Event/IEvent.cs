using MediatR;

namespace BuildingBlocks.Core.Event;

public interface IEvent : INotification
{
    Guid EventId => Guid.CreateVersion7();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().AssemblyQualifiedName;
}
