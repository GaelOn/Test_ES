using System;

namespace Domain.Base.Event.EventStore
{
    public interface IEventWrapper<TAggregateId>
    {
        IDomainEvent<TAggregateId> DomainEvent { get; }
        string Key { get; }
        long Version { get; }
        DateTime InsertDate { get; }
    }
}