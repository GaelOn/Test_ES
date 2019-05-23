using System;

namespace Domain.Base.Event.EventStore
{
    public class StoredVersionDontMatchException : EventStoreException
    {
        public StoredVersionDontMatchException() { }
        public StoredVersionDontMatchException(string message) : base(message) { }
        public StoredVersionDontMatchException(string message, Exception inner) : base(message, inner) { }
        protected StoredVersionDontMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
