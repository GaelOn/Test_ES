using Domain.Base.Event;
using Domain.Base.Event.EvantHandler;

namespace Domain.Base.Aggregate
{
    public abstract class EntityBase<TAggregateId, TEntityId> : IEntity<TAggregateId, TEntityId>, IEventDriven<TAggregateId>
    {
        #region private field
        IAggregateEventLifeCycleMedium<TAggregateId> _aggregateProxy;
        TAggregateId _streamId;
        #endregion

        #region protected ctor
        protected EntityBase(TEntityId id, BaseEventHandlerMap<TAggregateId> eventHandlerMap)
        {
            Id = id;
            EventHandlerMap = eventHandlerMap;
        }
        #endregion

        #region implementation of EntityBase<TAggregateId, TEntityId>
        public TEntityId Id { get; }

        #region Explicit implementation of EntityBase<TAggregateId, TEntityId>
        void IEntity<TAggregateId, TEntityId>.HookAggregate(IAggregateEventLifeCycleMedium<TAggregateId> aggregateProxy, TAggregateId aggregateId)
        {
            _streamId = aggregateId;
            _aggregateProxy = aggregateProxy;
        }
        #endregion
        #endregion

        #region purely from EntityBase<TAggregateId, TEntityId>
        protected BaseEventHandlerMap<TAggregateId> EventHandlerMap { get; }

        public void RaiseEvent<TEvent>(TEvent evt) where TEvent : DomainEventBase<TAggregateId>
        {
            _aggregateProxy.PrepareEvent(evt);
            ((IEventDriven<TAggregateId>)this).ProcessEvent(evt, _aggregateProxy.GetVersion()+1);
        }
        #endregion

        #region Explicit implementation of IEventDriven<TAggregateId>
        TAggregateId IEventDriven<TAggregateId>.StreamId => _streamId;

        void IEventDriven<TAggregateId>.ProcessEvent(IDomainEvent<TAggregateId> evt, long version)
        {
            IDomainEventHandler<TAggregateId> handler = null;
            bool existHandler()
            {
                handler = EventHandlerMap.GetHandlers(evt.GetType());
                return handler != null;
            }
            if (!_aggregateProxy.ExistUncommitedEvent(x => Equals(x.EventId, evt.EventId)) && existHandler())
            {
                handler.Continue(evt);
            }
        }
        #endregion

        #region protected method
        protected void processEvent(IDomainEventHandler<TAggregateId> handler, IDomainEvent<TAggregateId> evt, long version)
        {
            handler.ProcessEvent(evt);
            if(_aggregateProxy.TryCommitEventVersion(version))
            {
                _aggregateProxy.AddUncommitedEvent(evt);
            }
        }
        #endregion
    }
}
