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
    public class EventSourcedAggregateTransactionnalRepository<TAggregate, TAggregateId, TEntityId> : EventSourcedAggregateRepository<TAggregate, TAggregateId, TEntityId>,
            ITransactionnalSave<TAggregate, TAggregateId>
            where TAggregate : AggregateBase<TAggregateId, TEntityId>, new()
    {
        #region Private Field

        private readonly IIdProvider<TAggregateId> _idProvider;

        #endregion Private Field

        #region ctor

        public EventSourcedAggregateTransactionnalRepository(IEventStore<TAggregateId> eventStore,
                                                              IEventBus publisher,
                                                              IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> emptyAggregateFactory,
                                                              IIdProvider<TAggregateId> idProvider)
            : base(eventStore, publisher, emptyAggregateFactory) => _idProvider = idProvider;

        #endregion ctor

        #region ITransactionnalSave<TAggregate, TAggregateId>

        public TAggregate Save(TAggregate elem, IUnitOfWork<TAggregate, TAggregateId> uow)
        {
            var newAggregate = elem;
            var castedElem = ((IEventSourced<TAggregateId>)newAggregate);
            uow.OnCommit += castedElem.ClearUncommittedEvents;
            var tran = new EventStoreTransaction<TAggregate, TAggregateId>(_idProvider, uow);
            tran.BeginTransaction(castedElem.StreamId, castedElem.UncommittedEvents.ToList());
            var idEnumerator = (tran as IEnumerable<long>).GetEnumerator();
            var evtEnumerator = castedElem.UncommittedEvents.GetEnumerator();
            idEnumerator.MoveNext();
            evtEnumerator.MoveNext();
            try
            {
                do
                {
                    var evt = evtEnumerator.Current;
                    tran.ValidateEvent(idEnumerator.Current, evt);
                    _eventStore.AddEvent(evt);
                    PublishEvent(evt);
                } while (idEnumerator.MoveNext() && evtEnumerator.MoveNext());
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

        #endregion ITransactionnalSave<TAggregate, TAggregateId>
    }
}