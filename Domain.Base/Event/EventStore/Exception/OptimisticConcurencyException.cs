using System;

namespace Domain.Base.Event.EventStore
{
    [Serializable]
    public class OptimisticConcurencyException : Exception
    {
        public OptimisticConcurencyException() { }
        public OptimisticConcurencyException(string message) : base(message) { }
        public OptimisticConcurencyException(string message, Exception inner) : base(message, inner) { }
        protected OptimisticConcurencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
