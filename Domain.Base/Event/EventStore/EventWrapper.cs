using System;

namespace Domain.Base.Event.EventStore
{
    [Serializable]
    public class EventWrapper<TAggregateId> : IEquatable<EventWrapper<TAggregateId>>, IEventWrapper<TAggregateId>
    {
        #region private field
        private readonly Lazy<int> _hascode; 
        private readonly Lazy<string> _toStringValue; 
        #endregion

        #region Wrapper Functionnal Key
        public TAggregateId StreamId => DomainEvent.StreamId;
        public long Version => DomainEvent.EventVersion;
        public DateTime InsertDate { get; }
        #endregion

        #region ctor
        public EventWrapper(IDomainEvent<TAggregateId> domainEvent)
        {
            DomainEvent = domainEvent;
            InsertDate = DateTime.Now;
            Key = $"{StreamId}:{Version}:{InsertDate}";
            _toStringValue = new Lazy<string>(() => $"{Key}:{DomainEvent.EventId}");
            _hascode = new Lazy<int>(() => DomainEvent.EventId.GetHashCode());
        } 
        #endregion

        public string Key { get; }
        public IDomainEvent<TAggregateId> DomainEvent { get; }

        #region Object override and IEquatable<EventWrapper<TAggregateId>>
        public override bool Equals(object obj) => Equals(obj as EventWrapper<TAggregateId>);

        public override int GetHashCode() => _hascode.Value;

        public bool Equals(EventWrapper<TAggregateId> other) => other != null && DomainEvent.EventId == other.DomainEvent.EventId;

        public override string ToString() => _toStringValue.Value;
        #endregion
    }
}
