using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using static Domain.Base.Test.TestHelper.Scenario;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;
using FluentAssertions;
using Domain.Base.Event.IEventCommunication;
using Domain.Base.Event.EventStore;
using Domain.Base.Mock.CommunicationQueue;
using Domain.Base.DomainRepository;
using Domain.Base.DomainRepository.Transactional;
using Domain.Base.Mock.EventBus;
using Domain.Base.Mock;
using Domain.Base.Test.TestHelper;
using Domain.Base.Event.EventStore.IdProvider;
using Domain.Mock.Implem.UnitOfWork;

namespace Domain.Base.Test.RepositoryTest.Transactionnal
{
    [TestFixture]
    public class MultiThreadedEventSourcedAggregateTransactionnalRepositoryTest
    {
        protected IEventBus _bus;
        protected IEventStore<int> _store;
        protected IIdProvider<int> _idProvider;
        protected IHandlerRegister _eventListenerRegister;
        protected IDomainRepository<InputAggregate, int> _repo;
        protected ITransactionnalSave<InputAggregate, int> _saver;

        [SetUp]
        public void Initialize()
        {
            var q = new MockQ();
            _bus = new MockEventBus(q);
            _store = new EventStoreCrudMock<int>();
            _idProvider = new EventIdProvider<int>(_store);
            _repo = new TransactionnalInputAggregateRepo(_store, _bus, _idProvider);
            _saver = _repo as ITransactionnalSave<InputAggregate, int>;
            _eventListenerRegister = q.GetHandlerRegister("Mock");
        }

        [Test]
        public void Repo_Should_Rollback_Save_If_Not_on_The_Rigth_Version()
        {
            /// Arrange
            var param1 = GetArg(1);
            var param2 = GetArg(2);
            var aggregate = _repo.GetNewAggregate();
            var processElemCreation1 = new FirstSubProcess(param1.ProcessName, param1.ExpectedProcessId, param1.ExpectedDateCreated);
            var processElemCreation2 = new FirstSubProcess(param2.ProcessName, param2.ExpectedProcessId, param2.ExpectedDateCreated);
            var pingPong = new PingPong();
            var mre3 = new ManualResetEvent(false);
            var firstThreadFirstTaskHaveBeenRollbacked = false;
            var firstThreadSecondTaskHaveBeenRollbacked = false;
            var secondThreadFirstTaskHaveBeenRollbacked = false;

            void DoNothing()
            {
                return;
            }

            Action finalAction = DoNothing;
            void Thread1Work()
            {
                Thread.CurrentThread.Name = "Thread1Work";
                var uow1 = new BasicUnitOfWork<InputAggregate, int>();
                uow1.OnRollback += () => firstThreadFirstTaskHaveBeenRollbacked = true;
                var _aggregate1 = _repo.GetById(param1.ExpectedStreamId);
                pingPong.WaitPing();
                _aggregate1.RaiseEvent(new ProcessElementEntityCreated(param1.ExpectedStreamId, processElemCreation1));
                _aggregate1.RaiseEvent(new ProcessElemStarted(param1.ExpectedStreamId, param1.ExpectedProcessId,
                                                            param1.ExpectedRunningService, param1.ExpectedDateStarted));
                _aggregate1 = _saver.Save(_aggregate1, uow1);
                pingPong.Ping();
                var uow2 = new BasicUnitOfWork<InputAggregate, int>();
                uow1.OnRollback += () => firstThreadSecondTaskHaveBeenRollbacked = true;
                _aggregate1.RaiseEvent(new ProcessElemStoped(param1.ExpectedStreamId, param1.ExpectedProcessId, param1.ExpectedDateStoped));
                _aggregate1 = _saver.Save(_aggregate1, uow2);
                pingPong.SetOnlyPong();
            }
            void Thread2Work()
            {
                Thread.CurrentThread.Name = "Thread2Work";
                var uow1 = new BasicUnitOfWork<InputAggregate, int>();
                uow1.OnRollback += () => secondThreadFirstTaskHaveBeenRollbacked = true;
                var _aggregate2 = _repo.GetById(param1.ExpectedStreamId);
                pingPong.WaitPong();
                _aggregate2.RaiseEvent(new ProcessElementEntityCreated(param2.ExpectedStreamId, processElemCreation2));
                _aggregate2.RaiseEvent(new ProcessElemStarted(param2.ExpectedStreamId, param2.ExpectedProcessId,
                                                            param2.ExpectedRunningService, param2.ExpectedDateStarted));
                pingPong.Pong();
                _aggregate2.RaiseEvent(new ProcessElemStoped(param2.ExpectedStreamId, param2.ExpectedProcessId, param2.ExpectedDateStoped));
                _aggregate2 = _saver.Save(_aggregate2, uow1);
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
            firstThreadFirstTaskHaveBeenRollbacked.Should().BeFalse();
            firstThreadSecondTaskHaveBeenRollbacked.Should().BeFalse();
            secondThreadFirstTaskHaveBeenRollbacked.Should().BeTrue();
        }
    }
}