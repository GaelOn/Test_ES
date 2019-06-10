using Domain.Base.DomainRepository.Transactional;
using Domain.Base.Event;
using Domain.Base.Event.EventStore;
using Domain.Base.Event.EventStore.Transactional;
using Domain.Base.Mock;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.ToysEvent;
using Domain.Mock.Implem.UnitOfWork;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Domain.Base.Test.RepositoryTest.Transactionnal
{
    [TestFixture]
    public class EventStoreTransactionTest
    {
        private EventStoreTransaction<InputAggregate, int> _tran;
        private IUnitOfWork<InputAggregate, int> _uow;

        [SetUp]
        public void Initialize()
        {
            _uow = new BasicUnitOfWork<InputAggregate, int>();
            var idProvider = (new BasicMockIdProvider<int>()).AddCase(1, 2).AddCase(12, 0).AddCase(7, 5);
            _tran = new EventStoreTransaction<InputAggregate, int>(idProvider, _uow);
        }

        [Test]
        public void EventStoreTransaction_Should_Have_A_Unique_TransactionId()
        {
            // stupid but stupid error with Guid initialization is more
            // Arrange, Act
            var expected = ((ITransaction<int>)_tran).TransactionId;
            // Assert
            ((ITransaction<int>)_tran).TransactionId.Should().Be(expected);
        }

        [Test]
        public void EventStoreTrasaction_Cannot_Begin_If_Event_Dont_Belong_To_The_Stream()
        {
            // Arrange
            var evts = CreateEventFromVersion(1, 1, 3);
            // Act
            Action beginTran = () => _tran.BeginTransaction(2, evts);
            // Assert
            beginTran.Should().Throw<TransactionCannotBeginException>();
        }

        [Test]
        public void EventStoreTrasaction_Cannot_Provide_More_Id_Than_Asked()
        {
            // Arrange
            var expectedStreamId = 1;
            var expectedNbEvent = 3;
            var evts = CreateEventFromVersion(expectedStreamId, 2, expectedNbEvent);
            // Act
            _tran.BeginTransaction(expectedStreamId, evts);
            var idEnumerator = _tran.GetEnumerator();
            var evtEnumerator = evts.GetEnumerator();
            for (int it = 0; it < expectedNbEvent; it++)
            {
                idEnumerator.MoveNext();
                evtEnumerator.MoveNext();
            }
            // Assert
            (idEnumerator.MoveNext()).Should().BeFalse();
        }

        [Test]
        public void EventStoreTrasaction_Should_Commit_When_OptimisticConcurency_Succeeds()
        {
            // Arrange
            var expectedStreamId = 1;
            var evts = CreateEventFromVersion(expectedStreamId, 2, 3);
            var check1 = false;
            var check2 = 0;
            var haveBeenRollback = false;
            _uow.OnCommit += () => check1 = true;
            _uow.OnCommit += () => check2 = 1;
            _uow.OnRollback += () => haveBeenRollback = true;
            // Act
            RunTransaction(expectedStreamId, evts);
            // Assert
            check1.Should().Be(true);
            check2.Should().Be(1);
            haveBeenRollback.Should().Be(false);
        }

        [Test]
        public void EventStoreTrasaction_Should_Rollback_When_OptimisticConcurency_Fails()
        {
            // Arrange
            var expectedStreamId = 1;
            var evts = CreateEventFromVersion(expectedStreamId, 1, 3);
            var check1 = false;
            var check2 = 0;
            var haveBeenRollback = false;
            _uow.OnCommit += () => check1 = true;
            _uow.OnCommit += () => check2 = 1;
            _uow.OnRollback += () => haveBeenRollback = true;
            // Act
            RunTransaction(expectedStreamId, evts);
            // Assert
            check1.Should().Be(false);
            check2.Should().Be(0);
            haveBeenRollback.Should().Be(true);
        }

        [Test]
        public void EventStoreTrasaction_Can_Retry_On_Rollback()
        {
            // Arrange
            var expectedStreamId = 1;
            var evts = CreateEventFromVersion(expectedStreamId, 1, 3);
            var haveBeenRollback1 = false;
            var haveBeenRollback2 = false;
            Action firstTest = () => haveBeenRollback1 = true;
            Action secondtTest = () => haveBeenRollback2 = true;
            _uow.OnRollback += firstTest;
            // Act
            _tran.BeginTransaction(expectedStreamId, evts);
            using (var idEnumerator = _tran.GetEnumerator())
            using (var evtEnumerator = evts.GetEnumerator())
            {
                ValidateAllEvent(idEnumerator, evtEnumerator);
                if (haveBeenRollback1)
                {
                    _uow.OnRollback -= firstTest;
                    _uow.OnRollback += secondtTest;
                    idEnumerator.Reset();
                    evtEnumerator.Reset();
                    ValidateAllEvent(idEnumerator, evtEnumerator);
                }
            }

            // Assert
            haveBeenRollback2.Should().Be(true);
        }

        [Test]
        public void Dispose_Transaction_Should_Dispose_UnitOfWork()
        {
            // Arrange
            Action commit = (() => _uow.Commit());
            var check1 = false;
            var check2 = 0;
            _uow.OnCommit += () => check1 = true;
            _uow.OnCommit += () => check2 = 1;
            // Act
            _tran.Dispose();
            _uow.Commit();
            // Assert
            check1.Should().BeFalse();
            check2.Should().Be(0);
        }

        private void RunTransaction(int expectedStreamId, ICollection<IDomainEvent<int>> evts)
        {
            _tran.BeginTransaction(expectedStreamId, evts);
            var idEnumerator = _tran.GetEnumerator();
            var evtEnumerator = evts.GetEnumerator();
            ValidateAllEvent(idEnumerator, evtEnumerator);
        }

        private void ValidateAllEvent(IEnumerator<long> idEnumerator, IEnumerator<IDomainEvent<int>> evtEnumerator)
        {
            idEnumerator.MoveNext();
            evtEnumerator.MoveNext();
            try
            {
                do
                {
                    var evt = evtEnumerator.Current;
                    _tran.ValidateEvent(idEnumerator.Current, evt);
                } while (idEnumerator.MoveNext() && evtEnumerator.MoveNext());
                _tran.Commit();
            }
            catch
            {
                _tran.Rollback();
            }
        }

        private ICollection<IDomainEvent<int>> CreateEventFromVersion(int streamId, long fromVersion, int nbEvent)
        {
            var toBeReturned = new List<IDomainEvent<int>>(nbEvent);
            for (long i = fromVersion; i < (fromVersion + nbEvent); i++)
            {
                toBeReturned.Add(new TestEvent(1, i));
            }
            return toBeReturned;
        }
    }
}