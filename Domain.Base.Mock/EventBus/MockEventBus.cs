using System.Threading.Tasks;
using Domain.Base.Event;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.Mock.CommunicationQueue;

namespace Domain.Base.Mock.EventBus
{
    public class MockEventBus : IEventBus
    {
        private readonly IMessagePublisher _internalPublisher;

        public MockEventBus(ICommunicationQueue q) => _internalPublisher = q.GetMessagePublisher("Mock");

        public void PublishEvent<T, TAggregateId>(T evt) where T : IDomainEvent<TAggregateId> => _internalPublisher.Send(evt);

        public Task PublishEventAsync<T, TAggregateId>(T evt) where T : IDomainEvent<TAggregateId> 
            => Task.Factory.StartNew(() => PublishEvent<T, TAggregateId>(evt));
    }
}
