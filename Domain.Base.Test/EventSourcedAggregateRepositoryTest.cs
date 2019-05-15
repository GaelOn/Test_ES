using System;
using System.Linq;
using System.Threading;
using Domain.Base.Aggregate;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.DomainRepository;
using Domain.Base.Mock;
using Domain.Base.Mock.EventBus;
using Domain.Base.Mock.CommunicationQueue;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using Domain.Base.Test;
using static Domain.Base.Test.Scenario;
using NUnit.Framework;
using FluentAssertions;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    [TestFixture]
    public class EventSourcedAggregateRepositoryTest
    {
        private readonly IEventBus                              _bus;
        private readonly IEventStore<int>                       _store;
        private readonly IHandlerRegister                       _eventListenerRegister;
        private readonly IDomainRepository<InputAggregate, int> _repo;
        
        public EventSourcedAggregateRepositoryTest()
        {
            var q                  = new MockQ();
            _bus                   = new MockEventBus(q);
            _store                 = new EventStoreMock<int>();
            _repo                  = new InputAggregateRepo(_store, _bus);
            _eventListenerRegister = q.GetHandlerRegister("Mock");
        }

        [Test]
        public void Repo_Can_Create_An_Empty_Agregate()
        {
            // Arrange, Act
            var aggregate = _repo.GetNewAggregate();
            // Assert
            aggregate.Should().NotBeNull();
        }

        [Test]
        public void Repo_Should_Save_Event_On_Store()
        {
            // Arrange
            var aggregate = Start_Process(GetArg(), () => _repo.GetNewAggregate());
            // Act
            _repo.Save(aggregate);
            // Assert
            ((IEventSourced<int>)aggregate).UncommittedEvents.Count().Should().Be(0);
            _store.ReadEvents(1).Count().Should().Be(3);
        }

        [Test]
        public void Repo_Should_Publish_Event_On_Store()
        {
            // Arrange
            var aggregate = Start_Process(GetArg(), () => _repo.GetNewAggregate());
            var msgProcessor   = new MessageProcessor(_eventListenerRegister);
            msgProcessor.RegisterMessageToBeCounted<InputAggregateCreated>();
            msgProcessor.RegisterMessageToBeCounted<ProcessElementEntityCreated>();
            msgProcessor.RegisterMessageToBeCounted<ProcessElemStarted>();
            // Act
            _repo.Save(aggregate);
            // Assert
            ((IEventSourced<int>)aggregate).UncommittedEvents.Count().Should().Be(0);
            msgProcessor.Count.Should().Be(3);
        }


        [Test]
        public void Repo_Should_Restitute_Aggregate_At_the_Last_Save_State()
        {
            // Arrange
            var mre = new ManualResetEvent(false);
            ParamScenarioTest _param = GetArg();
            void Simulate_Something_ElseWhere()
            {
                var aggregate = Start_Process(_param, () => _repo.GetNewAggregate());
                _repo.Save(aggregate);
                mre.Set();
            }
            new Thread(Simulate_Something_ElseWhere).Start();
            mre.WaitOne();
            // Act
            var restoreAggregate = _repo.GetById(1);
            // Assert
            bool CriteriaOnRunningservice(IEntity<int, int> ent)
                => ent is FirstSubProcess && ((FirstSubProcess)ent).RunningService == _param.ExpectedRunningService;
            ((IEventSourced<int>)restoreAggregate).UncommittedEvents.Count().Should().Be(0);
            var entity = restoreAggregate.FindEntityByCriteria(CriteriaOnRunningservice);
        }
    }

    public class MessageProcessor
    {
        readonly IHandlerRegister _register;
        int _count;

        public int Count { get { return _count; } }

        public MessageProcessor(IHandlerRegister register) => _register = register;

        public void RegisterHandle<T>(Action<T> handle) => _register.RegisterHandler(handle);

        public void RegisterMessageToBeCounted<T>() => _register.RegisterHandler<T>(Increment);

        private void Increment<T>(T passiveEvt) => _count++;
    }
}
