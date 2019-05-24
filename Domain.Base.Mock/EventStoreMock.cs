using Domain.Base.Event;
using Domain.Base.Event.EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Base.Mock
{
    public class EventStoreMock<TStreamId> : IEventStore<TStreamId>
    {
        private readonly Dictionary<TStreamId, LinkedList<EventWrapper<TStreamId>>> _localStore 
            = new Dictionary<TStreamId, LinkedList<EventWrapper<TStreamId>>>(20);

        public NextExpectedVersionByStore AddEvent(IDomainEvent<TStreamId> evt)
        {
            if(!_localStore.ContainsKey(evt.StreamId))
            {
                _localStore[evt.StreamId] = new LinkedList<EventWrapper<TStreamId>>();
            }
            var _evts = _localStore[evt.StreamId];
            if (CheckEvent(_evts, evt.EventVersion))
            {
                _evts.AddLast(new EventWrapper<TStreamId>(evt));
            }
            else
            {
                throw new StoredVersionDontMatchException($"Event of type {evt.GetType()} should have a version number of {_evts.Last.Value.Version + 1} but found {evt.EventVersion}.");
            }
            return new NextExpectedVersionByStore(_localStore[evt.StreamId].Count);
        }

        public Task<NextExpectedVersionByStore> AddEventAsync(IDomainEvent<TStreamId> evt) => Task.Factory.StartNew(() => AddEvent(evt));

        public NextExpectedVersionByStore GetNextExpectedVersion(TStreamId streamId)
            => _localStore.ContainsKey(streamId) ? new NextExpectedVersionByStore(_localStore[streamId].Count) 
                                                 : new NextExpectedVersionByStore(0);

        public IEnumerable<IEventWrapper<TStreamId>> ReadEvents(TStreamId id)
            => _localStore.ContainsKey(id) ? _localStore[id].AsEnumerable<IEventWrapper<TStreamId>>() : null;

        public Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsAsync(TStreamId id) => Task.Factory.StartNew(() => ReadEvents(id));

        public IEnumerable<IEventWrapper<TStreamId>> ReadEventsFromVersion(TStreamId id, long versionId)
            => _localStore.ContainsKey(id) ? _localStore[id].Where((elem) => elem.DomainEvent.EventVersion >= versionId)
                                                            .OrderBy((elem) => elem.DomainEvent.EventVersion)
                                                            .AsEnumerable<IEventWrapper<TStreamId>>() : null;

        public Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsFromVersionAsync(TStreamId id, long versionId)
            => Task.Factory.StartNew(() => ReadEventsFromVersion(id, versionId));

        private bool CheckEvent(LinkedList<EventWrapper<TStreamId>> evts, long eventVersion)
            => (evts.Count == 0) ? eventVersion == 0 : evts.Last.Value.Version == eventVersion - 1;
    }
}
