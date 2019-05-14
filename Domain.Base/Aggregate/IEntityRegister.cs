using System;
using System.Collections.Generic;

namespace Domain.Base.Aggregate
{
    public interface IEntityProvider<TAggregateId, TEntityId>
    {
        void RegisterEntity(IEntity<TAggregateId, TEntityId> entity);
        IEntity<TAggregateId, TEntityId> FindEntityById(TEntityId id);

        IEnumerable<IEntity<TAggregateId, TEntityId>> FindEntityByCriteria(Func<IEntity<TAggregateId, TEntityId>, bool> criteria);
    }
}
