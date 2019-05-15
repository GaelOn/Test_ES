using Domain.Base;

namespace Domain.Mock.Implem.EventFromDomain.OfInput
{
    public sealed class ProcessElementEntityCreated : DomainEventBase<int>
    {
        public FirstSubProcess Process { get; }

        public ProcessElementEntityCreated(int streamId, FirstSubProcess process) : base(streamId) => Process = process;
    }
}
