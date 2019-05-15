using System;

namespace Domain.Base.Test
{
    public class ParamScenarioTest
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
            ProcessName = processName;
            ExpectedStreamId = expectedStreamId;
            ExpectedProcessId = expectedProcessId;
            ExpectedRunningService = expectedRunningService;
            ExpectedDateCreated = expectedDateCreated;
            ExpectedDateStarted = expectedDateStarted;
            ExpectedDateStoped = expectedDateStoped;
        }
    }
}
