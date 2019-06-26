namespace Domain.Base.Event.EventHandler
{
    public interface IDomainEventHandler<TId>
    {
        void ProcessEvent(IDomainEvent<TId> evt);

        void Continue(IDomainEvent<TId> evt);
    }

    public interface IDomainEventHandler<T, TId> : IDomainEventHandler<TId> where T : class, IDomainEvent<TId> { }
}