using Domain.Base;

namespace Domain.Mock.Implem.EventFromDomain.OfInput
{
    public sealed class InputAggregateCreated : DomainEventBase<int>
    {
        public InputAggregateCreated(int streamId) : base(streamId) { }

        public override void OfAggregate(IEventSourced<int> aggregate) => EventVersion = aggregate.CurrentVersion;
    }
}
