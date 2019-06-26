using System;
using System.Collections.Generic;

namespace Domain.Base.Event.EventHandler
{
    public abstract class BaseEventHandlerMap<TAggregateId> : IEventHandlerMap<TAggregateId>
    {
        private const int DefaultMapSize = 20;

        private readonly Dictionary<Type, IDomainEventHandler<TAggregateId>> _eventHandlerMap;

        protected BaseEventHandlerMap(int mapSize = DefaultMapSize)
            => _eventHandlerMap = new Dictionary<Type, IDomainEventHandler<TAggregateId>>(mapSize);

        public IDomainEventHandler<TAggregateId> GetHandlers(Type eventTypeToHandle)
            => _eventHandlerMap.ContainsKey(eventTypeToHandle) ? _eventHandlerMap[eventTypeToHandle] : null;

        public void RegisterHandle<T>(IDomainEventHandler<T, TAggregateId> handler) where T : class, IDomainEvent<TAggregateId>
            => _eventHandlerMap.Add(typeof(T), handler as IDomainEventHandler<TAggregateId>);
    }
}