using Domain.Base;
using System;

namespace Domain.Mock.Implem.EventFromDomain.EntityOfInput
{
    public class ProcessElemStarted : DomainEventBase<int>, IProcessElemEvent
    {
        public int ProcessElemId { get; }
        public string RunningService { get; }
        public DateTime Start { get; }

        public ProcessElemStarted(int streamId, int processElemId, string runningService, DateTime start) : base(streamId)
        {
            ProcessElemId = processElemId;
            RunningService = runningService;
            Start = start;
        }
    }
}
