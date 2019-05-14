using System;

namespace Domain.Base.Aggregate.AggregateException
{
    public class AggregateIdNotMatchException : Exception
    {

        public AggregateIdNotMatchException() : base("AggregateId (or StreamId) of the aggregate don't feed the application requirement.") { }
        public AggregateIdNotMatchException(string message) : base(message) { }
        public AggregateIdNotMatchException(string message, Exception inner) : base(message, inner) { }
        protected AggregateIdNotMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { } 

        public static AggregateIdNotMatchException GetAggregateIdNotMatchExceptionFromAggregateAndEvent<TAggregateId>(IEventSourced<TAggregateId> aggregate, 
                                                                                                                      DomainEventBase<TAggregateId> evt)
            => new AggregateIdNotMatchException(GetStandardizedErrorMessage(aggregate, evt));

        public static AggregateIdNotMatchException GetAggregateIdNotMatchExceptionFromAggregateAndEvent<TAggregateId>(IEventSourced<TAggregateId> aggregate,
                                                                                                                      DomainEventBase<TAggregateId> evt,
                                                                                                                      Exception inner)
            => new AggregateIdNotMatchException(GetStandardizedErrorMessage(aggregate, evt), inner);

        private static string GetStandardizedErrorMessage<TAggregateId>(IEventSourced<TAggregateId> aggregate, DomainEventBase<TAggregateId> evt)
            => $"The StreamId {aggregate.StreamId} of the aggregate don't match the recorded StreamId {evt.StreamId} of the Event.";
    }
}
