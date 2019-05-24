using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.DomainRepository;
using Domain.Base.Mock;
using Domain.Base.Mock.EventBus;
using Domain.Base.Mock.CommunicationQueue;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using NUnit.Framework;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    public abstract class BaseEventSourcedAggregateRepositoryTest
    {
        protected IEventBus _bus;
        protected IEventStore<int> _store;
        protected IHandlerRegister _eventListenerRegister;
        protected IDomainRepository<InputAggregate, int> _repo;

        [SetUp]
        public void Initialize()
        {
            var q = new MockQ();
            _bus = new MockEventBus(q);
            _store = new EventStoreMock<int>();
            _repo = new InputAggregateRepo(_store, _bus);
            _eventListenerRegister = q.GetHandlerRegister("Mock");
        }

        protected MessageProcessor InitializeMessageProcessor()
        {
            var msgProcessor = new MessageProcessor(_eventListenerRegister);
            msgProcessor.RegisterMessageToBeCounted<InputAggregateCreated>();
            msgProcessor.RegisterMessageToBeCounted<ProcessElementEntityCreated>();
            msgProcessor.RegisterMessageToBeCounted<ProcessElemStarted>();
            return msgProcessor;
        }
    }
}


