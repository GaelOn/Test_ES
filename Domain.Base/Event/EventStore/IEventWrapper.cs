namespace Domain.Base.Event.EventStore
{
    public interface IEventWrapper<TAggregateId>
    {
        IDomainEvent<TAggregateId> DomainEvent { get; }
        long EventNumber { get; }
        string Key { get; }
    }
}