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
        private const string CommunicationImpossibleWithPersistenceBackend = "Impossible to communicate persistence backend";
        private readonly IEventStore<TAggregateId> _eventStore;
        private readonly IEventBus _publisher;
        private readonly IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> _emptyAggregateFactory;
        private readonly Dictionary<Type, Action<object>> _publishMethods = new Dictionary<Type, Action<object>>(20);
        #endregion

        #region ctor
        public EventSourcedAggregateRepository(IEventStore<TAggregateId> eventStore,
                                              IEventBus publisher,
                                              IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> emptyAggregateFactory)
        {
            _eventStore = eventStore;
            _publisher = publisher;
            _emptyAggregateFactory = emptyAggregateFactory;
        }
        #endregion

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
                    aggregateEventSourcedView.ProcessEvent(evt.DomainEvent, evt.EventNumber);
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
                    aggregateEventSourcedView.ProcessEvent(evt.DomainEvent, evt.EventNumber);
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
                    await _eventStore.AddEventAsync(evt);
                    await _publisher.PublishEventAsync((dynamic)evt);
                }
                aggregateEventSourcedView.ClearUncommittedEvents();
            }
            catch (EventStoreNotReachableException ex)
            {
                throw new RepositoryException(CommunicationImpossibleWithPersistenceBackend, ex);
            }
        } 
        #endregion

        #region Private Method
        private void PublishEvent(IDomainEvent<TAggregateId> evt) => GetPublishMethod(evt.GetType())(evt);

        private Action<object> GetPublishMethod(Type type)
        {
            if (!_publishMethods.ContainsKey(type))
            {
                var action = GetDynamicPublishEventAction(type);
                _publishMethods[type] = (evt) => action(_publisher, evt);
            }
            return _publishMethods[type];
        }

        private Action<IEventBus, object> GetDynamicPublishEventAction(Type type)
        {
            var expParam           = Expression.Parameter(typeof(object));
            var expParamPublisher  = Expression.Parameter(typeof(IEventBus));
            var method             = typeof(IEventBus).GetMethod("PublishEvent", BindingFlags.Public
                                                                                 | BindingFlags.Instance
                                                                                 | BindingFlags.NonPublic);
            var expCall            = Expression.Call(expParamPublisher, method.MakeGenericMethod(new[] { type, typeof(TAggregateId) } ), Expression.Convert(expParam, type));
            var lambdaExp          = ((Expression<Action<IEventBus, object>>)Expression.Lambda(expCall, expParamPublisher, expParam));
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
        #endregion

        #endregion
    }
}
