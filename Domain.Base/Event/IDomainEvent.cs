
using System;

namespace Domain.Base.Event
{
    public interface IDomainEvent<TStreamId>
    {
        Guid EventId { get; }
        TStreamId StreamId { get; }
        long EventVersion { get; }
        void OfAggregate(IEventSourced<TStreamId> aggregate);
    }
}
