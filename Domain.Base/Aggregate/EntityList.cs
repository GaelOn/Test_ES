using System;
using System.Linq;
using System.Collections.Generic;

namespace Domain.Base.Aggregate
{
    internal class EntityList<TAggregatId, TEntityId> : IEntityList<TAggregatId, TEntityId>
    {
        private readonly List<IEntity<TAggregatId, TEntityId>> _internalList = new List<IEntity<TAggregatId, TEntityId>>(20);
        public int Count => _internalList.Count;

        public void Add(IEntity<TAggregatId, TEntityId> entity) => _internalList.Add(entity);

        public IEntity<TAggregatId, TEntityId> FindById(TEntityId entityId) => _internalList.FirstOrDefault(entity => entity.Id.Equals(entityId));

        public IEnumerable<IEntity<TAggregatId, TEntityId>> FindEntityByCriteria(Func<IEntity<TAggregatId, TEntityId>, bool> criteria) => _internalList.Where(criteria);

        public bool TryFindById(TEntityId entityId, out IEntity<TAggregatId, TEntityId> entity)
        {
            entity = _internalList.FirstOrDefault(internalEntity => internalEntity.Id.Equals(entityId));
            return entity != null;
        }
    }
}
