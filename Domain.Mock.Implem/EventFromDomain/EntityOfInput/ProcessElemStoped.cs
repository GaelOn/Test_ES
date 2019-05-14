using Domain.Base;
using System;

namespace Domain.Mock.Implem.EventFromDomain.EntityOfInput
{
    public class ProcessElemStoped : DomainEventBase<int>, IProcessElemEvent
    {
        public int ProcessElemId { get; }
        public DateTime Stop { get; }

        public ProcessElemStoped(int streamId, int processElemId, DateTime stop) : base(streamId)
        {
            ProcessElemId = processElemId;
            Stop = stop;
        }
    }
}
