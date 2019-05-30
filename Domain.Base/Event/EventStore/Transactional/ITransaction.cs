using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain.Base.Event.EventStore.Transactional
{
    public interface ITransaction<TStreamId> : IEnumerable<long>, IEnumerable
    {
        Guid TransactionId { get; }
        void BeginTransaction(TStreamId streamId, ICollection<IDomainEvent<TStreamId>> evts);
        void Commit();
        void Rollback();
        void ValidateEvent(long expectedId, IDomainEvent<TStreamId> evt);
    }
}
