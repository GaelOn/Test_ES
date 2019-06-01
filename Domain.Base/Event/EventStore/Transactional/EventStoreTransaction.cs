using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain.Base.DomainRepository.Transactional;
using Domain.Base.Event.EventStore.IdProvider;

namespace Domain.Base.Event.EventStore.Transactional
{
    public class EventStoreTransaction<TAggregate, TAggregateId> : ITransaction<TAggregateId>, IDisposable
    {
        #region Private fields
        private readonly IIdProvider<TAggregateId> _idProvider;
        private readonly IUnitOfWork<TAggregate, TAggregateId> _uow;
        private long[] _idRange;
        private readonly Guid _transactionId;
        #endregion

        #region ctor
        public EventStoreTransaction(IIdProvider<TAggregateId> idProvider, IUnitOfWork<TAggregate, TAggregateId> uow = null)
        {
            _idProvider = idProvider;
            _uow = uow;
            _transactionId = Guid.NewGuid();
        }
        #endregion

        #region Implementation of ITransaction<TStreamId>
        Guid ITransaction<TAggregateId>.TransactionId => _transactionId;

        public void BeginTransaction(TAggregateId streamId, ICollection<IDomainEvent<TAggregateId>> evts)
        {
            if (!evts.Aggregate(true, (res, evt) => res && (evt.StreamId.Equals(streamId))))
            {
                throw new TransactionCannotBeginException($"Try to start trasaction for StreamId {streamId} but not all events into collection belong to this stream.");
            }
            _idRange = _idProvider.PrepareIdRange(streamId, evts.Count);
        }

        public virtual void Commit()   => _uow?.Commit();
        public virtual void Rollback() => _uow?.Rollback(); 

        public void ValidateEvent(long expectedVersion, IDomainEvent<TAggregateId> evt)
        {
            if (expectedVersion != evt.EventVersion)
            {
                throw new OptimisticConcurencyException($"Event of type {evt.GetType()} should have a version number equal to {expectedVersion} but found {evt.EventVersion}.");
            }
        }

        public void Dispose() => _uow.Dispose();
        #endregion

        #region IEnumerable<long>
        public IEnumerator<long> GetEnumerator() => new IdRangeEnumerator(_idRange);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct IdRangeEnumerator : IEnumerator<long>, IEnumerator
        {
            private readonly int _max;
            private int _idx;
            private long[] _range;

            public IdRangeEnumerator(long[] range)
            {
                _range = range;
                _max = _range.Length;
                _idx = -1;
            }

            object IEnumerator.Current => _range[_idx];

            public long Current => _range[_idx];

            public void Dispose() => _range = null;

            public bool MoveNext()
            {
                if (_idx >= _max)
                {
                    return false;
                }
                _idx++;
                return true;
            }

            public void Reset() => _idx = 0;
        } 
        #endregion
    }
}
