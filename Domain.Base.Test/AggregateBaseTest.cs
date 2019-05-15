using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.DomainRepository;
using Domain.Base.Aggregate.AggregateException;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using static Domain.Base.Test.Scenario;
using NUnit.Framework;
using FluentAssertions;

namespace Domain.Base.AggregateBase.Test
{
    [TestFixture]
    public class AggregateBaseTest
    {
        private readonly IDomainRepository<InputAggregate, int> _repo;

        public AggregateBaseTest() => _repo = new InputAggregateRepo(null, null);

        [Test]
        public void Should_Create_An_Empty_Agregate()
        {
            /// Arrange
            var expectedStreamId = 1;
            var expectedNextCurrentVersion = 1;
            var expectedVersion = 0;
            var aggregate = _repo.GetNewAggregate();
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(expectedStreamId));
            /// Assert
            aggregate.AggregateId.Should().Be(expectedStreamId);
            ((IEventSourced<int>)aggregate).Version.Should().Be(expectedVersion);
            ((IEventSourced<int>)aggregate).CurrentVersion.Should().Be(expectedNextCurrentVersion);
        }


        [Test]
        public void Aggregate_Should_Throw_Exception_On_Bad_StreamId()
        {
            // Arrange
            var aggregate = _repo.GetNewAggregate();
            var paramScenarioTest = GetArg();
            var processElemCreation = new FirstSubProcess(paramScenarioTest.ProcessName,
                                                         paramScenarioTest.ExpectedProcessId,
                                                         paramScenarioTest.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(paramScenarioTest.ExpectedStreamId));
            // Act
            var save = new Action(() => aggregate.RaiseEvent(new ProcessElementEntityCreated(paramScenarioTest.ExpectedStreamId + 1, processElemCreation)));
            // Assert
            save.Should().Throw<AggregateIdNotMatchException>();
        }

        [Test]
        public void Should_Not_Processed_Not_Registered_Event()
        {
            /// Arrange
            var expectedStreamId = 1;
            var expectedNbEvent  = 1;
            var aggregate        = _repo.GetNewAggregate();
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(expectedStreamId));
            aggregate.RaiseEvent(new NotRegisteredEvent(expectedStreamId));
            /// Assert
            aggregate.AggregateId.Should().Be(expectedStreamId);
            ((IEventSourced<int>)aggregate).UncommittedEvents.Count().Should().Be(expectedNbEvent);
        }

        [Test]
        public void Should_Route_Event_To_The_Right_Underlying_Entity()
        {
            /// Arrange
            var expectedStreamId        = 1;
            var expectedProcessId       = 10;
            var aggregate               = _repo.GetNewAggregate();
            var expectedDateCreated     = DateTime.Now;
            var expectedDateStarted     = expectedDateCreated.AddMinutes(5);
            var expectedRunningService  = "TestService";
            var processElemCreation     = new FirstSubProcess("Test", expectedProcessId, expectedDateCreated);
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(expectedStreamId));
            aggregate.RaiseEvent(new ProcessElementEntityCreated(expectedStreamId, processElemCreation));
            aggregate.RaiseEvent(new ProcessElemStarted(expectedStreamId, expectedProcessId, expectedRunningService, expectedDateStarted));
            /// Assert
            var processElemRetrieve = aggregate.GetProcessElementById(expectedProcessId);
            processElemRetrieve.Should().NotBeNull();
            processElemRetrieve.RunningService.Should().Be(expectedRunningService);
            processElemRetrieve.Start.Should().Be(expectedDateStarted);
        }

        [Test]
        public void Should_Be_Handle_By_Two_Thread()
        {
            /// Arrange
            var expectedStreamId = 1;
            var expectedProcessId1 = 10;
            var expectedProcessId2 = 11;
            var aggregate = _repo.GetNewAggregate();
            var expectedDateCreated = DateTime.Now;
            var expectedDateStarted = expectedDateCreated.AddMinutes(5);
            var expectedRunningService = "TestService";
            var processElemCreation1 = new FirstSubProcess("Test1", expectedProcessId1, expectedDateCreated);
            var processElemCreation2 = new FirstSubProcess("Test2", expectedProcessId2, expectedDateCreated);
            var mre1 = new ManualResetEvent(true);
            var mre2 = new ManualResetEvent(false);
            var mre3 = new ManualResetEvent(false);
            void Thread1Work()
            {
                mre1.WaitOne();
                aggregate.RaiseEvent(new ProcessElementEntityCreated(expectedStreamId, processElemCreation1));
                aggregate.RaiseEvent(new ProcessElemStarted(expectedStreamId, expectedProcessId1, expectedRunningService, expectedDateStarted));
                mre2.Set();
                mre1.Reset();
                mre1.WaitOne();
                aggregate.RaiseEvent(new ProcessElemStoped(expectedStreamId, expectedProcessId1, expectedDateStarted));
                mre2.Set();
            }
            void Thread2Work()
            {
                mre2.WaitOne();
                aggregate.RaiseEvent(new ProcessElementEntityCreated(expectedStreamId, processElemCreation2));
                aggregate.RaiseEvent(new ProcessElemStarted(expectedStreamId, expectedProcessId2, expectedRunningService, expectedDateStarted));
                mre1.Set();
                mre2.Reset();
                mre2.WaitOne();
                aggregate.RaiseEvent(new ProcessElemStoped(expectedStreamId, expectedProcessId2, expectedDateStarted));
            }
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(expectedStreamId));
            var t1 = Task.Factory.StartNew(Thread1Work);
            var t2 = Task.Factory.StartNew(Thread2Work);
            var tasks = new[] { t1, t2 };
            Task.WhenAll(tasks).ContinueWith((t) => mre3.Set());
            mre3.WaitOne();
            /// Assert
            var processElemRetrieve = aggregate.GetProcessElementById(expectedProcessId1);
            processElemRetrieve.Should().NotBeNull();
            processElemRetrieve.RunningService.Should().Be(expectedRunningService);
            processElemRetrieve.Start.Should().Be(expectedDateStarted);
        }
    }
}
