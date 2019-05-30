namespace Domain.Base.Event.EventStore.IdProvider
{
    public interface IIdProvider<StreamId>
    {
        StreamId GetStreamId();
        long GetNextId(StreamId key);
        long PrepareId(StreamId key);
        long[] PrepareIdRange(StreamId key, int rangeSize);
        void CommitId(StreamId key, long id);
    }
}
