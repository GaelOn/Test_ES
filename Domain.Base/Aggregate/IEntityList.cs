using System;
using System.Collections.Generic;

namespace Domain.Base.Aggregate
{
    public interface IEntityList<TAggregateId, TEntityId>
    {
        IEntity<TAggregateId, TEntityId> FindById(TEntityId entityId);
        IEnumerable<IEntity<TAggregateId, TEntityId>> FindEntityByCriteria(Func<IEntity<TAggregateId, TEntityId>, bool> criteria);
        bool TryFindById(TEntityId entityId, out IEntity<TAggregateId, TEntityId> entity);
        void Add(IEntity<TAggregateId, TEntityId> entity);
        int Count { get; }
    }
}
