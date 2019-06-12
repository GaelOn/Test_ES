using Domain.Base.Mock;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Base.Test.RepositoryTest.Transactionnal
{
    [TestFixture]
    public class EventIdProviderTest
    {
        private EventIdProvider<int> _eventIdProvider;

        [SetUp]
        public void Initialize()
        {
            var eventStore = new BasicMockEventStore();
            _eventIdProvider = new EventIdProvider<int>(eventStore);
            foreach (var cas in AlltestCase.CaseObjectPrepareId)
            {
                int streamId = (int)((object[])cas)[0];
                int versionId = (int)((object[])cas)[1];
                eventStore.AddCase(streamId, versionId);
            }
        }

        [TestCaseSource(typeof(AlltestCase), "CaseObjectPrepareId")]
        public void Should_Return_VersionId_When_PrepareId_IsCall(int expectedStreamId, long expectedVersionId)
        {
            // Act
            var versionId = _eventIdProvider.PrepareId(expectedStreamId);
            // Assert
            versionId.Should().Be(expectedVersionId);
        }

        [TestCaseSource(typeof(AlltestCase), "CaseObjectPrepareRangeId")]
        public void Should_Return_VersionId_When_PrepareIdRange_IsCall(int expectedStreamId, long expectedVersionId, int expectedSize, long[] expectedRangeId)
        {
            // Act
            var versionsId = _eventIdProvider.PrepareIdRange(expectedStreamId, expectedSize);
            // Assert
            versionsId.Should().BeEquivalentTo(expectedRangeId);
        }
    }

    internal class AlltestCase
    {
        public static object[] CaseObjectPrepareId =
        {
             new object[] { 1, 1 },
             new object[] { 5, 23 }
        };

        public static IEnumerable CaseObjectPrepareRangeId
        {
            get
            {
                yield return new TestCaseData(1, (long)1, 3, new long[] { 1, 2, 3 });
                yield return new TestCaseData(5, (long)23, 8, new long[] { 23, 24, 25, 26, 27, 28, 29, 30 });
            }
        }
    }
}