using System;

namespace Domain.Base.Event.EventStore
{
    [Serializable]
    public class EventStoreException : Exception
    {
        /// <summary>
        /// EventStore exception not related to specialized one. For example, you can log error related to the backend 
        /// use as persistence solution (like mongo DB, CouchBase etc...)
        /// </summary>
        public EventStoreException() { }
        public EventStoreException(string message) : base(message) { }
        public EventStoreException(string message, Exception inner) : base(message, inner) { }
        protected EventStoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
