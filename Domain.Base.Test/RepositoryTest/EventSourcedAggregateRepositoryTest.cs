using System.Linq;
using System.Threading;
using Domain.Base.Aggregate;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using static Domain.Base.Test.TestHelper.Scenario;
using NUnit.Framework;
using FluentAssertions;
using Domain.Base.Test.TestHelper;
using System.Threading.Tasks;

namespace Domain.Base.Test.RepositoryTest
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
        public void Repo_Can_Not_Create_An_Empty_Agregate_Using_GetById()
        {
            // Arrange, Act
            var aggregate = _repo.GetById(10);
            // Assert
            aggregate.Should().BeNull();
        }

        [Test]
        public async Task Repo_Can_Not_Create_An_Empty_Agregate_Using_GetByIdAsync()
        {
            // Arrange, Act
            AggregateBase<int, int> aggregate = _repo.GetById(10);
            aggregate = await _repo.GetByIdAsync(10).ConfigureAwait(false);
            // Assert
            aggregate.Should().BeNull();
        }

        [Test]
        public async Task Repo_Can_Retrieve_Agregate_Using_GetByIdAsync()
        {
            // Arrange
            var param = GetArg(1);
            var aggregate = _repo.GetNewAggregate();
            var processElemCreation = new FirstSubProcess(param.ProcessName, param.ExpectedProcessId, param.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(param.ExpectedStreamId));
            aggregate.RaiseEvent(new ProcessElementEntityCreated(param.ExpectedStreamId, processElemCreation));
            aggregate.RaiseEvent(new ProcessElemStarted(param.ExpectedStreamId, param.ExpectedProcessId,
                                                        param.ExpectedRunningService, param.ExpectedDateStarted));
            aggregate.RaiseEvent(new ProcessElemStoped(param.ExpectedStreamId, param.ExpectedProcessId, param.ExpectedDateStoped));
            _repo.Save(aggregate);
            // Act
            var aggregate2 = await _repo.GetByIdAsync(param.ExpectedStreamId);
            // Assert
            aggregate2.GetProcessElementById(param.ExpectedProcessId).ShouldBeAsExpected(param);
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
        public async Task Repo_Should_Save_Asynchronously_Event_On_Store()
        {
            // Arrange
            var aggregate = Start_Process(GetArg(), () => _repo.GetNewAggregate());
            // Act
            await _repo.SaveAsync(aggregate);
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