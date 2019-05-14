namespace Domain.Base.Aggregate
{
    public interface IEntity<TAggregateId, TEntityId>
    {
        TEntityId Id { get; }
        void HookAggregate(IAggregateEventLifeCycleMedium<TAggregateId> aggregateProxy, TAggregateId aggregateId);
    }
}
