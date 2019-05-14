namespace Domain.Base.Aggregate
{
    public interface IAggregate<TId>
    {
        TId AggregateId { get; }
    }
}
