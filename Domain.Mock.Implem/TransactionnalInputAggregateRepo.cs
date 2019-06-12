using Domain.Base.Aggregate;
using Domain.Base.DomainRepository.Transactional;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;

namespace Domain.Mock.Implem
{
    //public class TransactionnalInputAggregateRepo : EventSourcedAggrregateTransactionnalRepository<InputAggregate, int, int>
    //{
    //    public TransactionnalInputAggregateRepo(IEventStore<int> store, IEventBus bus) : base(store, bus, new DefaultEmptyAggregateFactory<InputAggregate, int, int>())
    //    {
    //    }
    //}
}