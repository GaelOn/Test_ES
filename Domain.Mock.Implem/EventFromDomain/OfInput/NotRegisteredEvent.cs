using Domain.Base;

namespace Domain.Mock.Implem
{
    public sealed class NotRegisteredEvent : DomainEventBase<int>
    {
        public NotRegisteredEvent(int streamId) : base(streamId) { }
    }
}
