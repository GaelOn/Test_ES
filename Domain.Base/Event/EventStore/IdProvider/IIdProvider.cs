namespace Domain.Base.Event.EventStore.IdProvider
{
    public interface IIdProvider<StreamId>
    {
        long PrepareId(StreamId key);

        long[] PrepareIdRange(StreamId key, int rangeSize);
    }
}