using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Base.Event.EventStore
{
    public interface IEventStore<TStreamId>
    {
        IEnumerable<IEventWrapper<TStreamId>> ReadEvents(TStreamId id);
        IEnumerable<IEventWrapper<TStreamId>> ReadEventsFromVersion(TStreamId id, long versionId);
        NextExpectedVersionByStore AddEvent(IDomainEvent<TStreamId> evt);

        Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsAsync(TStreamId id);
        Task<IEnumerable<IEventWrapper<TStreamId>>> ReadEventsFromVersionAsync(TStreamId id, long versionId);
        Task<NextExpectedVersionByStore> AddEventAsync(IDomainEvent<TStreamId> evt);

        NextExpectedVersionByStore GetNextExpectedVersion(TStreamId streamId);
    }
}
