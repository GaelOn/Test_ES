using System.Collections.Generic;
using Domain.Base.Event;
using Domain.Base.DomainRepository.Transactional;

namespace Domain.Mock.Implem.UnitOfWork
{
    public class BasicUnitOfWork<TAgregate, TAgregateId> : BaseUnitOfWork<TAgregate, TAgregateId>
    {
        public sealed override TAgregate BeforeRollback(IEnumerable<IDomainEvent<TAgregateId>> evts) => default;
    }
}
