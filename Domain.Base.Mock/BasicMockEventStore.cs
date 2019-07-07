using Domain.Base.Event;
using Domain.Base.Event.EventStore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Base.Mock
{
    public class BasicMockEventStore : IEventStore<int>
    {
        private Dictionary<int, long> _caseStore = new Dictionary<int, long>();

        public NextExpectedVersionByStore AddEvent(IDomainEvent<int> evt)
        {
            throw new NotImplementedException();
        }

        public Task<NextExpectedVersionByStore> AddEventAsync(IDomainEvent<int> evt)
        {
            throw new NotImplementedException();
        }

        public NextExpectedVersionByStore GetNextExpectedVersion(int streamId)
            => _caseStore.ContainsKey(streamId) ? new NextExpectedVersionByStore(_caseStore[streamId]) : null;

        public IEnumerable<IEventWrapper<int>> ReadEvents(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventWrapper<int>>> ReadEventsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEventWrapper<int>> ReadEventsFromVersion(int id, long versionId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventWrapper<int>>> ReadEventsFromVersionAsync(int id, long versionId)
        {
            throw new NotImplementedException();
        }

        public bool TryAddEventWithUniqueKey(IDomainEvent<int> evt, Func<IDomainEvent<int>, bool> CompareEvt)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryAddEventWithUniqueKeyAsync(IDomainEvent<int> evt, Func<IDomainEvent<int>, bool> CompareEvt)
        {
            throw new NotImplementedException();
        }

        public BasicMockEventStore AddCase(int streamId, long currentVersion)
        {
            if (!_caseStore.ContainsKey(streamId))
            {
                _caseStore.Add(streamId, currentVersion);
            }
            else
            {
                _caseStore[streamId] = currentVersion;
            }
            return this;
        }
    }
}