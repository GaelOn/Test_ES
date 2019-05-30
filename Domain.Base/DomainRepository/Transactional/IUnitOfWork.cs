using System;
using System.Collections.Generic;
using Domain.Base.Event;

namespace Domain.Base.DomainRepository.Transactional
{
    public interface IUnitOfWork<TAggregate, TAggregateId> : IDisposable
    {
        TAggregate BeforeRollback(IEnumerable<IDomainEvent<TAggregateId>> evts);

        event Action OnCommit;
        event Action OnRollback;

        void Commit();
        void Rollback();
    }
}
