using System;
using System.Linq;
using Domain.Base.Event.EventStore;
using Domain.Base.DomainRepository;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.Mock;
using Domain.Base.Mock.EventBus;
using Domain.Base.Mock.CommunicationQueue;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using NUnit.Framework;
using FluentAssertions;
using System.Threading;
using Domain.Base.Aggregate;

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
            var aggregate = Scenario_Start_Process();
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
            var (aggregate, _) = Scenario_Start_Process();
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

        private (InputAggregate, ParamScenarioTest) Scenario_Start_Process()
        {
            var paramScenarioTest      = GetArg();
            var aggregate              = _repo.GetNewAggregate();
            var processElemCreation    = new ProcessElement(paramScenarioTest.ProcessName, 
                                                            paramScenarioTest.ExpectedProcessId, 
                                                            paramScenarioTest.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(paramScenarioTest.ExpectedProcessId));
            aggregate.RaiseEvent(new ProcessElementEntityCreated(paramScenarioTest.ExpectedStreamId, 
                                                                 processElemCreation));
            aggregate.RaiseEvent(new ProcessElemStarted(paramScenarioTest.ExpectedStreamId, 
                                                        paramScenarioTest.ExpectedProcessId, 
                                                        paramScenarioTest.ExpectedRunningService, 
                                                        paramScenarioTest.ExpectedDateStarted));
            return (aggregate, paramScenarioTest);
        }

        private class ParamScenarioTest
        {
            public int ExpectedStreamId { get; }
            public int ExpectedProcessId { get; }
            public DateTime ExpectedDateCreated { get; }
            public DateTime ExpectedDateStarted { get; }
            public DateTime ExpectedDateStoped { get; }
            public string ProcessName { get; }
            public string ExpectedRunningService { get; }
            public ParamScenarioTest(string processName, int expectedStreamId, int expectedProcessId, 
                                     string expectedRunningService, DateTime expectedDateCreated, 
                                     DateTime expectedDateStarted, DateTime expectedDateStoped)
            {
                ProcessName            = processName;
                ExpectedStreamId       = expectedStreamId;
                ExpectedProcessId      = expectedProcessId;
                ExpectedRunningService = expectedRunningService;
                ExpectedDateCreated    = expectedDateCreated;
                ExpectedDateStarted    = expectedDateStarted;
                ExpectedDateStoped     = expectedDateStoped;
            }
        }

        private ParamScenarioTest GetArg()
        {
            return new ParamScenarioTest("Test", 1, 10, "TestService", DateTime.Now, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(5));
        }


        [Test]
        public void Repo_Should_Restitute_Aggregate_At_the_Last_Save_State()
        {
            // Arrange
            var mre = new ManualResetEvent(false);
            ParamScenarioTest _param = null;
            void Simulate_Something_ElseWhere(out ParamScenarioTest param)
            {
                var (aggregate, param) = Scenario_Start_Process();
                _repo.Save(aggregate);
                mre.Set();
            }
            new Thread(Simulate_Something_ElseWhere).Start();
            mre.WaitOne();
            // Act
            var restoreAggregate = _repo.GetById(1);
            // Assert
            bool CriteriaOnRunningservice(IEntity<int, int> ent)
                => ent is ProcessElement && ((ProcessElement)ent).RunningService == "TestService";
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
