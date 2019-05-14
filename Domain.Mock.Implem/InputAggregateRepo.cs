using Domain.Base.DomainRepository;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.Aggregate;

namespace Domain.Mock.Implem
{
    public class InputAggregateRepo : EventSourcedAggregateRepository<InputAggregate, int, int>
    {
        public InputAggregateRepo(IEventStore<int> store, IEventBus bus) : base(store, bus, new DefaultEmptyAggregateFactory<InputAggregate, int, int>()) { }
    }
}
