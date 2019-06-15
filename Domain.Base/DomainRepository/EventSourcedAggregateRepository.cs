using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Base.Event;
using Domain.Base.Aggregate;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;

namespace Domain.Base.DomainRepository
{
    public class EventSourcedAggregateRepository<TAggregate, TAggregateId, TEntityId> : IDomainRepository<TAggregate, TAggregateId>
        where TAggregate : AggregateBase<TAggregateId, TEntityId>, new()
    {
        #region Private Field

        protected const string CommunicationImpossibleWithPersistenceBackend = "Impossible to communicate persistence backend";
        protected readonly IEventStore<TAggregateId> _eventStore;
        protected readonly IEventBus _publisher;
        protected readonly IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> _emptyAggregateFactory;
        protected readonly Dictionary<Type, Action<object>> _publishMethods = new Dictionary<Type, Action<object>>(20);

        #endregion Private Field

        #region ctor

        public EventSourcedAggregateRepository(IEventStore<TAggregateId> eventStore,
                                              IEventBus publisher,
                                              IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> emptyAggregateFactory)
        {
            _eventStore = eventStore;
            _publisher = publisher;
            _emptyAggregateFactory = emptyAggregateFactory;
        }

        #endregion ctor

        #region IDomainRepository<TAggregate, TAggregateId> implementation

        public TAggregate GetById(TAggregateId id)
        {
            try
            {
                var aggregate = _emptyAggregateFactory.GetEmptyAggregate();
                IEventSourced<TAggregateId> aggregateEventSourcedView = aggregate;

                foreach (var evt in _eventStore.ReadEvents(id))
                {
                    //aggregate.RaiseEvent(evt.DomainEvent);
                    aggregateEventSourcedView.ProcessEvent(evt.DomainEvent, evt.Version);
                }
                aggregateEventSourcedView.ClearUncommittedEvents();
                return aggregate;
            }
            catch (AggregateNotFoundEventStoreException)
            {
                return null;
            }
            catch (EventStoreNotReachableException ex)
            {
                throw new RepositoryException(CommunicationImpossibleWithPersistenceBackend, ex);
            }
        }

        public async Task<TAggregate> GetByIdAsync(TAggregateId id)
        {
            try
            {
                var aggregate = _emptyAggregateFactory.GetEmptyAggregate();
                IEventSourced<TAggregateId> aggregateEventSourcedView = aggregate;

                foreach (var evt in await _eventStore.ReadEventsAsync(id))
                {
                    aggregateEventSourcedView.ProcessEvent(evt.DomainEvent, evt.Version);
                }
                return aggregate;
            }
            catch (AggregateNotFoundEventStoreException)
            {
                return null;
            }
            catch (EventStoreNotReachableException ex)
            {
                throw new RepositoryException(CommunicationImpossibleWithPersistenceBackend, ex);
            }
        }

        public TAggregate GetNewAggregate() => _emptyAggregateFactory.GetEmptyAggregate();

        public void Save(TAggregate aggregate)
        {
            try
            {
                IEventSourced<TAggregateId> aggregateEventSourcedView = aggregate;
                var versionFromStore = _eventStore.GetNextExpectedVersion(aggregate.AggregateId);
                long count = 0;
                foreach (var evt in aggregateEventSourcedView.UncommittedEvents)
                {
                    VersionGuard(versionFromStore, evt);
                    versionFromStore = _eventStore.AddEvent(evt);
                    PublishEvent(evt);
                    count++;
                }
                aggregateEventSourcedView.ClearUncommittedEvents();
            }
            catch (EventStoreNotReachableException ex)
            {
                throw new RepositoryException(CommunicationImpossibleWithPersistenceBackend, ex);
            }
        }

        public async Task SaveAsync(TAggregate aggregate)
        {
            try
            {
                IEventSourced<TAggregateId> aggregateEventSourcedView = aggregate;

                foreach (var evt in aggregateEventSourcedView.UncommittedEvents)
                {
                    await _eventStore.AddEventAsync(evt).ConfigureAwait(false);
                    // TODO : create PublishEventAsync method to make this call async
                    PublishEvent(evt);
                }
                aggregateEventSourcedView.ClearUncommittedEvents();
            }
            catch (EventStoreNotReachableException ex)
            {
                throw new RepositoryException(CommunicationImpossibleWithPersistenceBackend, ex);
            }
        }

        #endregion IDomainRepository<TAggregate, TAggregateId> implementation

        protected void PublishEvent(IDomainEvent<TAggregateId> evt) => GetPublishMethod(evt.GetType(), "PublishEvent")(evt);

        #region Private Method

        private Action<object> GetPublishMethod(Type type, string methodName)
        {
            if (!_publishMethods.ContainsKey(type))
            {
                var action = GetDynamicPublishEventAction(type, methodName);
                _publishMethods[type] = (evt) => action(_publisher, evt);
            }
            return _publishMethods[type];
        }

        private Action<IEventBus, object> GetDynamicPublishEventAction(Type type, string methodName)
        {
            var expParam = Expression.Parameter(typeof(object));
            var expParamPublisher = Expression.Parameter(typeof(IEventBus));
            var method = typeof(IEventBus).GetMethod(methodName, BindingFlags.Public
                                                                             | BindingFlags.Instance
                                                                             | BindingFlags.NonPublic);
            var expCall = Expression.Call(expParamPublisher, method.MakeGenericMethod(new[] { type, typeof(TAggregateId) }), Expression.Convert(expParam, type));
            var lambdaExp = ((Expression<Action<IEventBus, object>>)Expression.Lambda(expCall, expParamPublisher, expParam));
            return lambdaExp.Compile();
        }

        #region Guard methods

        private void VersionGuard(NextExpectedVersionByStore expectedVersion, IDomainEvent<TAggregateId> evt)
        {
            if (expectedVersion.ExpectedVersion != evt.EventVersion)
            {
                throw new StoredVersionDontMatchException($"Event of type {evt.GetType()} should have a version number equal to {expectedVersion} but found {evt.EventVersion}.");
            }
        }

        #endregion Guard methods

        #endregion Private Method
    }
}