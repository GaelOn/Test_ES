using Domain.Base.Event;
using Domain.Base.Event.EventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Base.Mock
{
    public class EventStoreCrudMock<TStreamId> : IEventStore<TStreamId>
    {
        private static ConcreteStore<TStreamId, EventWrapper<TStreamId>> _localStore;

        static EventStoreCrudMock() => _localStore = new ConcreteStore<TStreamId, EventWrapper<TStreamId>>();

        public EventStoreCrudMock()
        {
        }

        public NextExpectedVersionByStore AddEvent(IDomainEvent<TStreamId> evt)
        {
            var wrapper = new EventWrapper<TStreamId>(evt);
            _localStore.Insert(evt.StreamId, wrapper);
            var evts = _localStore.Select(evt.StreamId, (wrap => wrap.DomainEvent.EventVersion == evt.EventVersion));
            if (evts.Length > 1 && evts.FirstOrDefault(wrapevt => wrapevt.Version == evt.EventVersion &&
                                                                  wrapevt.InsertDate == evts.Min(wrappedEvt => wrappedEvt.InsertDate))?.DomainEvent != evt)
            {
                _localStore.Delete(wrapper, wrap => wrap.StreamId, wrap => wrap.DomainEvent.EventId == evt.EventId);
                var nev = GetNextExpectedVersionAsLong(evt.StreamId);
                throw new StoredVersionDontMatchException($"Event of type {evt.GetType()} intend a version number equal to {nev} but found {evt.EventVersion}.");
            }
            return GetNextExpectedVersion(evt.StreamId);
        }

        public Task<NextExpectedVersionByStore> AddEventAsync(IDomainEvent<TStreamId> evt) => Task.Factory.StartNew(() => AddEvent(evt));

        public NextExpectedVersionByStore GetNextExpectedVersion(TStreamId streamId) => new NextExpectedVersionByStore(GetNextExpectedVersionAsLong(streamId));

        private long GetNextExpectedVersionAsLong(TStreamId streamId) => ReadEvents(streamId)?.Max(wrap => wrap.Version) + 1 ?? 0;

        public IEnumerable<IEventWrapper<TStreamId>> ReadEvents(TStreamId streamId)
            => _localStore.Select(streamId, wrap => true);

        public Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsAsync(TStreamId id) => Task.Factory.StartNew(() => ReadEvents(id));

        public IEnumerable<IEventWrapper<TStreamId>> ReadEventsFromVersion(TStreamId streamId, long versionId)
            => ReadEvents(streamId).Where((elem) => elem.DomainEvent.EventVersion >= versionId)
                                   .OrderBy((elem) => elem.DomainEvent.EventVersion);

        public Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsFromVersionAsync(TStreamId id, long versionId)
            => Task.Factory.StartNew(() => ReadEventsFromVersion(id, versionId));

        public bool TryAddEventWithUniqueKey(IDomainEvent<TStreamId> evt, Func<IDomainEvent<TStreamId>, bool> compareEvt)
        {
            var wrapper = new EventWrapper<TStreamId>(evt);
            _localStore.Insert(evt.StreamId, wrapper);
            var evts = _localStore.Select(evt.StreamId, (wrap => wrap.DomainEvent.EventVersion == evt.EventVersion));
            if (evts.Length > 1 && evts.FirstOrDefault(wrapevt => compareEvt(wrapevt.DomainEvent) &&
                                                                  wrapevt.InsertDate == evts.Min(wrappedEvt => wrappedEvt.InsertDate))?.DomainEvent != evt)
            {
                // rollback the insertion because we found a previously inserted event
                _localStore.Delete(wrapper, wrap => wrap.StreamId);
                return false;
            }
            return true;
        }

        public Task<bool> TryAddEventWithUniqueKeyAsync(IDomainEvent<TStreamId> evt, Func<IDomainEvent<TStreamId>, bool> compareEvt)
            => Task.Factory.StartNew(() => TryAddEventWithUniqueKey(evt, compareEvt));

        private bool CheckEvent(LinkedList<EventWrapper<TStreamId>> evts, long eventVersion)
            => (evts.Count == 0) ? eventVersion == 0 : evts.Last.Value.Version == eventVersion - 1;

        public void Reset() => EventStoreCrudMock<TStreamId>._localStore = new ConcreteStore<TStreamId, EventWrapper<TStreamId>>();
    }
}