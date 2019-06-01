using System;

namespace Domain.Base.Event.EventStore
{
    public class TransactionCannotBeginException : Exception
    {
        /// <summary>
        /// When a transaction failed for other reason than OptimisticConcurency (like opening a transaction 
        /// using a StreamId and event using other one).
        /// </summary>
        public TransactionCannotBeginException() { }
        public TransactionCannotBeginException(string message) : base(message) { }
        public TransactionCannotBeginException(string message, Exception inner) : base(message, inner) { }
        protected TransactionCannotBeginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
