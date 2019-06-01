using Domain.Base;

namespace Domain.Mock.Implem.EventFromDomain.ToysEvent
{
    public class TestEvent : DomainEventBase<int>
    {
        public TestEvent(int streamId, long version) : base(streamId, version) { }
    }
}
