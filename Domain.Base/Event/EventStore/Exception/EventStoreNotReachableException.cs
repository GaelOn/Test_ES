using System;

namespace Domain.Base.Event.EventStore
{
    /// <summary>
    /// Maybe issue with Communication layer or Server is reachable but instance is not.
    /// </summary>
    [Serializable]
    public class EventStoreNotReachableException : EventStoreException
    {
        public EventStoreNotReachableException() { }
        public EventStoreNotReachableException(string message) : base(message) { }
        public EventStoreNotReachableException(string message, Exception inner) : base(message, inner) { }
        protected EventStoreNotReachableException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
