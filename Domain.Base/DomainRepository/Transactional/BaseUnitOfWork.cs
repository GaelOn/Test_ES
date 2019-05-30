using System;
using System.Threading;
using System.Collections.Generic;
using Domain.Base.Event;

namespace Domain.Base.DomainRepository.Transactional
{
    public abstract class BaseUnitOfWork<TAggregate, TAggregateId> : IUnitOfWork<TAggregate, TAggregateId>
    {

        #region ctor
        protected BaseUnitOfWork() { }
        #endregion

        #region Event
        public event Action OnCommit;

        public event Action OnRollback; 
        #endregion

        public abstract TAggregate BeforeRollback(IEnumerable<IDomainEvent<TAggregateId>> evts);
        public virtual void Commit()   => Volatile.Read(ref OnCommit)?.Invoke();
        public virtual void Rollback() => Volatile.Read(ref OnRollback)?.Invoke();

        public virtual void Dispose()
        {
            OnCommit   = null;
            OnRollback = null;
        }
    }
}
