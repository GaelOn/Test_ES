using Domain.Base.Event.EventStore;
using Domain.Base.Event.EventStore.IdProvider;

namespace Domain.Base.Mock
{
    public class EventIdProvider<TStreamId> : IIdProvider<TStreamId>
    {
        private IEventStore<TStreamId> _evtStore;

        public EventIdProvider(IEventStore<TStreamId> evtStore) => _evtStore = evtStore;

        public long PrepareId(TStreamId key) => _evtStore.GetNextExpectedVersion(key).ExpectedVersion;

        public long[] PrepareIdRange(TStreamId key, int rangeSize)
        {
            var start = _evtStore.GetNextExpectedVersion(key).ExpectedVersion;
            var toBeReturned = new long[rangeSize];
            for (int i = 0; i < rangeSize; i++)
            {
                toBeReturned[i] = start + i;
            }
            return toBeReturned;
        }
    }
}