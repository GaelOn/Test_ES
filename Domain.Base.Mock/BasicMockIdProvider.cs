using System.Collections.Generic;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.EventStore.IdProvider;

namespace Domain.Base.Mock
{
    public class BasicMockIdProvider<TId> : IIdProvider<TId>
    {
        private readonly Dictionary<TId, long> _caseMap = new Dictionary<TId, long>(20);

        public void CommitId(TId key, long id)
        {
            if (_caseMap[key] == id)
            {
                throw new OptimisticConcurencyException($"Test failed, current version number equal to {id} should have been memoized but found {_caseMap[key]}.");
            }
            ++_caseMap[key];
        }

        public long PrepareId(TId key) => _caseMap[key];

        public long[] PrepareIdRange(TId key, int rangeSize)
        {
            var toBeReturned = new long[rangeSize];
            for (int i = 0; i < rangeSize; i++)
            {
                toBeReturned[i] = _caseMap[key] + i;
            }
            return toBeReturned;
        }

        public BasicMockIdProvider<TId> AddCase(TId id, long versionStart)
        {
            if (!_caseMap.ContainsKey(id))
            {
                _caseMap.Add(id, versionStart);
            }
            return this;
        }
    }
}
