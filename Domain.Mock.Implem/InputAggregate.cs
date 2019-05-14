using System.Linq;
using System.Collections.Generic;
using Domain.Base;
using Domain.Base.Event;
using Domain.Base.Aggregate;
using Domain.Base.Event.EvantHandler;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;

namespace Domain.Mock.Implem
{
    public class InputAggregate : AggregateBase<int, int>
    {
        public List<ProcessElement> ProcessElements { get; }

        public InputAggregate() : base(new InputEventProcessor())
        {
            ((InputEventProcessor)EventHandlerMap).InAggregate = this;
        }

        private class InputEventProcessor : BaseEventHandlerMap<int>
        {
            public InputAggregate InAggregate { get; set; }

            public InputEventProcessor()
            {
                RegisterHandle(new DomainEventHandler<InputAggregateCreated, int>(HandleCreationEvent, RouteToAggregate));
                RegisterHandle(new DomainEventHandler<ProcessElementEntityCreated, int>(HandleProcessEventCreationEvent, RouteToAggregate));
                RegisterHandle(new DomainEventHandler<ProcessElemStarted, int>(null, RouteProcessElemStartedToProcessElement));
                RegisterHandle(new DomainEventHandler<ProcessElemStoped, int>(null, RouteProcessElemStopedToProcessElement));
            }

            public void HandleCreationEvent(InputAggregateCreated evt) 
                => InAggregate.AggregateId = evt.StreamId;

            public void HandleProcessEventCreationEvent(ProcessElementEntityCreated evt) 
                => ((IEntityProvider<int, int>)InAggregate).RegisterEntity(evt.Process);

            public void HandleStartProcessElemEvent(ProcessElemStarted evt) 
                => RouteEventToProcessElement(evt, evt.ProcessElemId);

            private void RouteEventToProcessElement(DomainEventBase<int> evt, int id) 
                => (InAggregate.FindEntityById(id) as ProcessElement).RaiseEvent(evt);

            private void RouteProcessElemStartedToProcessElement(IDomainEventHandler<int> handler, IDomainEvent<int> evt)
                => RouteProcessElemToProcessElement<ProcessElemStarted>(handler, evt);

            private void RouteProcessElemStopedToProcessElement(IDomainEventHandler<int> handler, IDomainEvent<int> evt)
                => RouteProcessElemToProcessElement<ProcessElemStoped>(handler, evt);

            private void RouteProcessElemToProcessElement<T>(IDomainEventHandler<int> handler, IDomainEvent<int> evt)
                where T : DomainEventBase<int>, IProcessElemEvent
            {
                var castedEvt = evt as T;
                (InAggregate.FindEntityById((castedEvt).ProcessElemId) as ProcessElement).RaiseEvent(castedEvt);
            }

            private void RouteToAggregate(IDomainEventHandler<int> handler, IDomainEvent<int> evt)
                => InAggregate.processEvent(handler, evt, evt.EventVersion + 1);
        }

        public ProcessElement GetProcessElementById(int processId) => FindEntityById(processId) as ProcessElement;
    }
}
