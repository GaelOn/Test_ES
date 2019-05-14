using System;

namespace Domain.Base.Event.EvantHandler
{
    public interface IEventHandlerMap<TId>
    {
        IDomainEventHandler<TId> GetHandlers(Type eventTypeToHandle);

        void RegisterHandle<T>(IDomainEventHandler<T, TId> handler) where T : class, IDomainEvent<TId>;
    }
}
