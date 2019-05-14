namespace Domain.Base.Aggregate
{
    public interface IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> where TAggregate : AggregateBase<TAggregateId, TEntityId>, new()
    {
        TAggregate GetEmptyAggregate();
    }
}
