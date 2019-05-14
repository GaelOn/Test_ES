using System;

namespace Domain.Base.Event.EvantHandler
{
    public interface IDomainEventHandler<TId>
    { 
        void ProcessEvent(IDomainEvent<TId> evt);
        void Continue(IDomainEvent<TId> evt);
    }

    public interface IDomainEventHandler<T, TId> : IDomainEventHandler<TId> where T : class, IDomainEvent<TId> { }
}
