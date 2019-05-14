using Domain.Base;

namespace Domain.Mock.Implem.EventFromDomain.OfInput
{
    public sealed class ProcessElementEntityCreated : DomainEventBase<int>
    {
        public ProcessElement Process { get; }

        public ProcessElementEntityCreated(int streamId, ProcessElement process) : base(streamId) => Process = process;
    }
}
