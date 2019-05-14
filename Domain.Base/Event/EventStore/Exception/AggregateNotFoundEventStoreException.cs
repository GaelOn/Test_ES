using System;

namespace Domain.Base.Event.EventStore
{
    /// <summary>
    /// Can not found Event related to the StreamId/AggregateId.
    /// </summary>
    [Serializable]
    public class AggregateNotFoundEventStoreException : EventStoreException
    {
        public AggregateNotFoundEventStoreException() { }
        public AggregateNotFoundEventStoreException(string message) : base(message) { }
        public AggregateNotFoundEventStoreException(string message, Exception inner) : base(message, inner) { }
        protected AggregateNotFoundEventStoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
