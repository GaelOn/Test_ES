using Domain.Base;

namespace Domain.Mock.Implem.EventFromDomain.OfInput
{
    public sealed class InputAggregateCreated : DomainEventBase<int>
    {
        public InputAggregateCreated(int streamId) : base(streamId) { }

        public InputAggregateCreated(int streamId, int version) : base(streamId, version) { }

        public override void OfAggregate(IEventSourced<int> aggregate) => EventVersion = aggregate.CurrentVersion;
    }
}
