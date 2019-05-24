using System.Linq;
using System.Threading;
using Domain.Base.Aggregate;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using Domain.Base.Test;
using Domain.Base.AggregateBase.Test;
using static Domain.Base.Test.Scenario;
using NUnit.Framework;
using FluentAssertions;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    [TestFixture]
    public partial class EventSourcedAggregateRepositoryTest : BaseEventSourcedAggregateRepositoryTest
    {

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
        public void Repo_Allow_To_Add_Event_On_Reloaded_Aggregate()
        {
            // Arrange
            var param = GetArg(1);
            var aggregate = _repo.GetNewAggregate();
            var processElemCreation = new FirstSubProcess(param.ProcessName, param.ExpectedProcessId, param.ExpectedDateCreated);
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(param.ExpectedStreamId));
            _repo.Save(aggregate);
            var aggregate2 = _repo.GetById(param.ExpectedStreamId);
            aggregate2.RaiseEvent(new ProcessElementEntityCreated(param.ExpectedStreamId, processElemCreation));
            aggregate2.RaiseEvent(new ProcessElemStarted(param.ExpectedStreamId, param.ExpectedProcessId,
                                                         param.ExpectedRunningService, param.ExpectedDateStarted));
            aggregate2.RaiseEvent(new ProcessElemStoped(param.ExpectedStreamId, param.ExpectedProcessId, param.ExpectedDateStoped));
            _repo.Save(aggregate2);
            // Assert
            ((IEventSourced<int>)aggregate2).UncommittedEvents.Count().Should().Be(0);
            _store.ReadEvents(1).Count().Should().Be(4);
            aggregate2.GetProcessElementById(param.ExpectedProcessId).ShouldBeAsExpected(param);
        }

        [Test]
        public void Repo_Should_Publish_Event_On_Store()
        {
            // Arrange
            var aggregate = Start_Process(GetArg(), () => _repo.GetNewAggregate());
            MessageProcessor msgProcessor = InitializeMessageProcessor();
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
}


