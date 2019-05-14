namespace Domain.Base.Event.EventStore
{
    public class NextExpectedVersionByStore
    {
        public long ExpectedVersion { get; }

        public NextExpectedVersionByStore(long expectedVersion) => ExpectedVersion = expectedVersion;
    }
}
