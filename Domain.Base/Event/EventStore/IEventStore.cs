using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Base.Event.EventStore.IdProvider;

namespace Domain.Base.Event.EventStore
{
    public interface IEventStore<TStreamId>
    {
        IEnumerable<IEventWrapper<TStreamId>> ReadEvents(TStreamId id);
        IEnumerable<IEventWrapper<TStreamId>> ReadEventsFromVersion(TStreamId id, long versionId);
        NextExpectedVersionByStore AddEvent(IDomainEvent<TStreamId> evt);
        bool TryAddEventWithUniqueKey(IDomainEvent<TStreamId> evt, Func<IDomainEvent<TStreamId>, bool> CompareEvt);

        Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsAsync(TStreamId id);
        Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsFromVersionAsync(TStreamId id, long versionId);
        Task<NextExpectedVersionByStore> AddEventAsync(IDomainEvent<TStreamId> evt);
        Task<bool> TryAddEventWithUniqueKeyAsync(IDomainEvent<TStreamId> evt, Func<IDomainEvent<TStreamId>, bool> CompareEvt);

        NextExpectedVersionByStore GetNextExpectedVersion(TStreamId streamId);
    }

    public interface ITransactionalEventStore<TStreamId> : IEventStore<TStreamId>, IIdProvider<TStreamId>
    {

    }
}
