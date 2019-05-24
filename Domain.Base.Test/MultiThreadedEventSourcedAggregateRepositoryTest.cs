using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using static Domain.Base.Test.Scenario;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using FluentAssertions;
using Domain.Base.AggregateBase.Test;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    [TestFixture]
    public class MultiThreadedEventSourcedAggregateRepositoryTest : BaseEventSourcedAggregateRepositoryTest
    {
        [Test]
        public void Repo_Should_Failed_To_Updated_Aggregate_When_Used_NonSequentialy_WithOut_Protection_By_Two_Thread()
        {
            /// Arrange
            var param1 = GetArg(1);
            var param2 = GetArg(2);
            var aggregate = _repo.GetNewAggregate();
            var processElemCreation1 = new FirstSubProcess(param1.ProcessName, param1.ExpectedProcessId, param1.ExpectedDateCreated);
            var processElemCreation2 = new FirstSubProcess(param2.ProcessName, param2.ExpectedProcessId, param2.ExpectedDateCreated);
            var pingPong = new PingPong();
            var mre3 = new ManualResetEvent(false);
            void DoNothing()
            {
                return;
            }

            Action finalAction = DoNothing;
            void Thread1Work()
            {
                Thread.CurrentThread.Name = "Thread1Work";
                var _aggregate1 = _repo.GetById(param1.ExpectedStreamId);
                pingPong.WaitPing();
                _aggregate1.RaiseEvent(new ProcessElementEntityCreated(param1.ExpectedStreamId, processElemCreation1));
                _aggregate1.RaiseEvent(new ProcessElemStarted(param1.ExpectedStreamId, param1.ExpectedProcessId,
                                                            param1.ExpectedRunningService, param1.ExpectedDateStarted));
                _repo.Save(_aggregate1);
                pingPong.Ping();
                _aggregate1.RaiseEvent(new ProcessElemStoped(param1.ExpectedStreamId, param1.ExpectedProcessId, param1.ExpectedDateStoped));
                _repo.Save(_aggregate1);
                pingPong.SetOnlyPong();
            }
            void Thread2Work()
            {
                Thread.CurrentThread.Name = "Thread2Work";
                var _aggregate2 = _repo.GetById(param1.ExpectedStreamId);
                pingPong.WaitPong();
                _aggregate2.RaiseEvent(new ProcessElementEntityCreated(param2.ExpectedStreamId, processElemCreation2));
                _aggregate2.RaiseEvent(new ProcessElemStarted(param2.ExpectedStreamId, param2.ExpectedProcessId,
                                                            param2.ExpectedRunningService, param2.ExpectedDateStarted));
                pingPong.Pong();
                _aggregate2.RaiseEvent(new ProcessElemStoped(param2.ExpectedStreamId, param2.ExpectedProcessId, param2.ExpectedDateStoped));
                _repo.Save(_aggregate2);
            }
            void finalTask(Task t)
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception;
                    finalAction = () => throw ex;
                }
                mre3.Set();
            }
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(param1.ExpectedStreamId));
            _repo.Save(aggregate);
            var t1 = Task.Factory.StartNew(Thread1Work);
            var t2 = Task.Factory.StartNew(Thread2Work);
            var tasks = new[] { t1, t2 };
            Task.WhenAll(tasks).ContinueWith(finalTask);
            mre3.WaitOne();
            /// Assert
            var _aggregate = _repo.GetById(param1.ExpectedStreamId);
            _aggregate.GetProcessElementById(param1.ExpectedProcessId).ShouldBeAsExpected(param1);
            finalAction.Should().Throw<AggregateException>();
        }

        [Test]
        public void Repo_Should_Failed_To_Updated_Aggregate_When_Used_Sequentialy_WithOut_Protection_By_Two_Thread()
        {
            /// Arrange
            var param1 = GetArg(1);
            var param2 = GetArg(2);
            var aggregate = _repo.GetNewAggregate();
            var processElemCreation1 = new FirstSubProcess(param1.ProcessName, param1.ExpectedProcessId, param1.ExpectedDateCreated);
            var processElemCreation2 = new FirstSubProcess(param2.ProcessName, param2.ExpectedProcessId, param2.ExpectedDateCreated);
            var pingPong = new PingPong();
            var mre3 = new ManualResetEvent(false);
            void DoNothing()
            {
                return;
            }

            Action finalAction = DoNothing;
            void Thread1Work()
            {
                Thread.CurrentThread.Name = "Thread1Work";
                var _aggregate1 = _repo.GetById(param1.ExpectedStreamId);
                pingPong.WaitPing();
                _aggregate1.RaiseEvent(new ProcessElementEntityCreated(param1.ExpectedStreamId, processElemCreation1));
                _aggregate1.RaiseEvent(new ProcessElemStarted(param1.ExpectedStreamId, param1.ExpectedProcessId,
                                                            param1.ExpectedRunningService, param1.ExpectedDateStarted));
                _repo.Save(_aggregate1);
                _aggregate1.RaiseEvent(new ProcessElemStoped(param1.ExpectedStreamId, param1.ExpectedProcessId, param1.ExpectedDateStoped));
                _repo.Save(_aggregate1);
                pingPong.Ping();
                pingPong.SetOnlyPong();
            }
            void Thread2Work()
            {
                Thread.CurrentThread.Name = "Thread2Work";
                pingPong.WaitPong();
                var _aggregate2 = _repo.GetById(param1.ExpectedStreamId);
                _aggregate2.RaiseEvent(new ProcessElementEntityCreated(param2.ExpectedStreamId, processElemCreation2));
                _aggregate2.RaiseEvent(new ProcessElemStarted(param2.ExpectedStreamId, param2.ExpectedProcessId,
                                                            param2.ExpectedRunningService, param2.ExpectedDateStarted));
                _aggregate2.RaiseEvent(new ProcessElemStoped(param2.ExpectedStreamId, param2.ExpectedProcessId, param2.ExpectedDateStoped));
                _repo.Save(_aggregate2);
                pingPong.Pong();
            }
            void finalTask(Task t)
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception;
                    finalAction = () => throw ex;
                }
                mre3.Set();
            }
            // Act
            aggregate.RaiseEvent(new InputAggregateCreated(param1.ExpectedStreamId));
            _repo.Save(aggregate);
            var t1 = Task.Factory.StartNew(Thread1Work);
            var t2 = Task.Factory.StartNew(Thread2Work);
            var tasks = new[] { t1, t2 };
            Task.WhenAll(tasks).ContinueWith(finalTask);
            mre3.WaitOne();
            /// Assert
            var _aggregate = _repo.GetById(param1.ExpectedStreamId);
            _aggregate.GetProcessElementById(param1.ExpectedProcessId).ShouldBeAsExpected(param1);
            finalAction.Should().NotThrow();
            _aggregate.GetProcessElementById(param2.ExpectedProcessId).ShouldBeAsExpected(param2);
        }
    }
}
