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
            var param1 = GetArg(1);
            var param2 = GetArg(2);
            var aggregate = _repo.GetNewAggregate();
            //var expectedDateCreated = DateTime.Now;
            //var expectedDateStarted = expectedDateCreated.AddMinutes(5);
            var processElemCreation1 = new FirstSubProcess(param1.ProcessName, param1.ExpectedProcessId, param1.ExpectedDateCreated);
            var processElemCreation2 = new FirstSubProcess(param2.ProcessName, param2.ExpectedProcessId, param2.ExpectedDateCreated);
            var mre1 = new ManualResetEvent(true);
            var mre2 = new ManualResetEvent(false);
            var mre3 = new ManualResetEvent(false);
            void Thread1Work()
            {
                mre1.WaitOne();
                aggregate.RaiseEvent(new ProcessElementEntityCreated(param1.ExpectedStreamId, processElemCreation1));
                aggregate.RaiseEvent(new ProcessElemStarted(param1.ExpectedStreamId, param1.ExpectedProcessId, 
                                                            param1.ExpectedRunningService, param1.ExpectedDateStarted));
                mre2.Set();
                mre1.Reset();
                mre1.WaitOne();
                aggregate.RaiseEvent(new ProcessElemStoped(param1.ExpectedStreamId, param1.ExpectedProcessId, param1.ExpectedDateStoped));
                mre2.Set();
            }
            void Thread2Work()
            {
                mre2.WaitOne();
                aggregate.RaiseEvent(new ProcessElementEntityCreated(param2.ExpectedStreamId, processElemCreation2));
                aggregate.RaiseEvent(new ProcessElemStarted(param2.ExpectedStreamId, param2.ExpectedProcessId,
                                                            param2.ExpectedRunningService, param2.ExpectedDateStarted));
                mre1.Set();
                mre2.Reset();
                mre2.WaitOne();
                aggregate.RaiseEvent(new ProcessElemStoped(param2.ExpectedStreamId, param2.ExpectedProcessId, param2.ExpectedDateStoped));
            }
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(param1.ExpectedStreamId));
            var t1 = Task.Factory.StartNew(Thread1Work);
            var t2 = Task.Factory.StartNew(Thread2Work);
            var tasks = new[] { t1, t2 };
            Task.WhenAll(tasks).ContinueWith((t) => mre3.Set());
            mre3.WaitOne();
            /// Assert
            aggregate.GetProcessElementById(param1.ExpectedProcessId).ShouldBeAsExpected(param1);
            aggregate.GetProcessElementById(param2.ExpectedProcessId).ShouldBeAsExpected(param2);
        }
    }
}
