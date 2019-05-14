using System;
using System.Linq;
using System.Collections.Generic;
using Domain.Base.Event;
using Domain.Base.Event.EvantHandler;

namespace Domain.Base.Aggregate
{
    public abstract class AggregateBase<TAggregateId, TEntityId> : IAggregate<TAggregateId>, IEventSourced<TAggregateId>, IEntityProvider<TAggregateId, TEntityId>
    {
        public const long WhenNewAggregate_StartVersion = 0;

        #region Private fields
        private readonly ICollection<IDomainEvent<TAggregateId>> _uncommittedEvents = new List<IDomainEvent<TAggregateId>>(20);
        private          long _version                                              = WhenNewAggregate_StartVersion;
        private          long _currentVersion                                       = WhenNewAggregate_StartVersion;
        private readonly IEntityList<TAggregateId, TEntityId> _entityList           = new EntityList<TAggregateId, TEntityId>();
        #endregion

        #region protected ctor
        protected AggregateBase(BaseEventHandlerMap<TAggregateId> eventHandlerMap)
            => EventHandlerMap = eventHandlerMap; 
        #endregion

        #region IAggregate<TAggregateId> implementation
        public TAggregateId AggregateId { get; protected set; }
        #endregion

        #region Purely from the abstract class
        protected BaseEventHandlerMap<TAggregateId> EventHandlerMap { get; }

        public void RaiseEvent<TEvent>(TEvent evt) where TEvent : DomainEventBase<TAggregateId>
        {
            evt.OfAggregate(this);
            ((IEventSourced<TAggregateId>)this).ProcessEvent(evt, _currentVersion + 1);
        } 
        #endregion

        #region Explicit implementation of IEventSourced<TAggregateId>

        long IEventSourced<TAggregateId>.Version => _version;
        long IEventSourced<TAggregateId>.CurrentVersion => _currentVersion;
        IEnumerable<IDomainEvent<TAggregateId>> IEventSourced<TAggregateId>.UncommittedEvents => _uncommittedEvents;

        TAggregateId IEventSourced<TAggregateId>.StreamId => AggregateId;

        void IEventSourced<TAggregateId>.ProcessEvent(IDomainEvent<TAggregateId> evt, long version)
        {
            IDomainEventHandler<TAggregateId> handler = null;
            bool existHandler()
            {
                handler = EventHandlerMap.GetHandlers(evt.GetType());
                return handler != null;
            }
            if (!_uncommittedEvents.Any(x => Equals(x.EventId, evt.EventId)) && existHandler())
            {
                handler.Continue(evt);
            }
        }
        void IEventSourced<TAggregateId>.ClearUncommittedEvents() => _uncommittedEvents.Clear();

        #endregion

        #region IEntityRegister<TAggregateId, TEntityId> explicit implementation
        void IEntityProvider<TAggregateId, TEntityId>.RegisterEntity(IEntity<TAggregateId, TEntityId> entity)
        {
            entity.HookAggregate(new AggregateEventMedium(this), AggregateId);
            _entityList.Add(entity);
        }

        public IEntity<TAggregateId, TEntityId> FindEntityById(TEntityId id) => _entityList.FindById(id);

        public IEnumerable<IEntity<TAggregateId, TEntityId>> FindEntityByCriteria(Func<IEntity<TAggregateId, TEntityId>, bool> criteria) => _entityList.FindEntityByCriteria(criteria);
        #endregion

        #region AggregateEventMedium private class to be passed at entity instance during registration
        private class AggregateEventMedium : IAggregateEventLifeCycleMedium<TAggregateId>
        {
            AggregateBase<TAggregateId, TEntityId> _aggregate;
            public AggregateEventMedium(AggregateBase<TAggregateId, TEntityId> aggregate) => _aggregate = aggregate;
            public long GetVersion() => _aggregate._currentVersion;
            public void PrepareEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent<TAggregateId> => evt.OfAggregate(_aggregate);
            public void AddUncommitedEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent<TAggregateId> => _aggregate._uncommittedEvents.Add(evt);
            public bool ExistUncommitedEvent(Func<IDomainEvent<TAggregateId>, bool> existCriteria) => _aggregate._uncommittedEvents.Any(existCriteria);
            public bool TryCommitEventVersion(long version)
            {
                if (_aggregate._currentVersion + 1 != version)
                {
                    return false;
                }
                _aggregate._currentVersion = version;
                return true;
            }

        }
        #endregion

        #region protected method
        protected void processEvent(IDomainEventHandler<TAggregateId> handler, IDomainEvent<TAggregateId> evt, long version)
        {
            handler.ProcessEvent(evt);
            _currentVersion = version;
            _uncommittedEvents.Add(evt);
        }
        #endregion
    }
}
