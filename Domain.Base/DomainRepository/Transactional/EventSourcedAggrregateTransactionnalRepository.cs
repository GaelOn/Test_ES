using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Base.Aggregate;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.Event.EventStore.IdProvider;
using Domain.Base.Event.EventStore.Transactional;

namespace Domain.Base.DomainRepository.Transactional
{
    public sealed class EventSourcedAggrregateTransactionnalRepository<TAggregate, TAggregateId, TEntityId> : EventSourcedAggregateRepository<TAggregate, TAggregateId, TEntityId>,
            ITransactionnalSave<TAggregate, TAggregateId>
            where TAggregate : AggregateBase<TAggregateId, TEntityId>, new()
    {
        #region Private Field
        private readonly IIdProvider<TAggregateId> _idProvider; 
        #endregion

        #region ctor
        public EventSourcedAggrregateTransactionnalRepository(IEventStore<TAggregateId> eventStore,
                                                              IEventBus publisher,
                                                              IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> emptyAggregateFactory,
                                                              IIdProvider<TAggregateId> idProvider)
            : base(eventStore, publisher, emptyAggregateFactory) => _idProvider = idProvider;
        #endregion

        #region ITransactionnalSave<TAggregate, TAggregateId>
        public TAggregate Save(TAggregate elem, IUnitOfWork<TAggregate, TAggregateId> uow)
        {
            var newAggregate  = elem;
            var castedElem    = ((IEventSourced<TAggregateId>)newAggregate);
            var tran          = new EventStoreTransaction<TAggregate, TAggregateId>(_idProvider, uow);
            var idEnumerator  = (tran as IEnumerable<long>).GetEnumerator();
            var evtEnumerator = castedElem.UncommittedEvents.GetEnumerator();
            evtEnumerator.MoveNext();
            tran.BeginTransaction(castedElem.StreamId, castedElem.UncommittedEvents.ToList());
            try
            {
                do
                {
                    var evt = evtEnumerator.Current;
                    tran.ValidateEvent(idEnumerator.Current, evt);
                    _eventStore.AddEvent(evt);
                    PublishEvent(evt);
                } while (idEnumerator.MoveNext() && evtEnumerator.MoveNext());
                castedElem.ClearUncommittedEvents();
                tran.Commit();
            }
            //TODO : find what to do with the exception.
            catch 
            {
                tran.Rollback();
                newAggregate = GetById(newAggregate.AggregateId);
            }
            return newAggregate;
        }

        public Task<TAggregate> SaveAsync(TAggregate elem, IUnitOfWork<TAggregate, TAggregateId> uow)
        {
            var task = new Task<TAggregate>(() => Save(elem, uow));
            task.Start();
            return task;
        }
        #endregion
    }
}
