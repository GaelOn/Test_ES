using System;

namespace Domain.Base.Event.EventStore
{
    [Serializable]
    public class EventWrapper<TAggregateId> : IEquatable<EventWrapper<TAggregateId>>, IEventWrapper<TAggregateId>
    {
        #region Wrapper Key
        public TAggregateId StreamId => DomainEvent.StreamId;
        public long Version => DomainEvent.EventVersion;
        #endregion

        public EventWrapper(IDomainEvent<TAggregateId> domainEvent)
        {
            DomainEvent = domainEvent;
            Key = $"{StreamId}:{Version}";
        }

        public string Key { get; }
        public long EventNumber { get; private set; }
        public IDomainEvent<TAggregateId> DomainEvent { get; }

        #region Object override and IEquatable<EventWrapper<TAggregateId>>
        public override bool Equals(object obj) => Equals(obj as EventWrapper<TAggregateId>);

        public override int GetHashCode() => Key.GetHashCode();

        public bool Equals(EventWrapper<TAggregateId> other) => other != null && Key.Equals(other.Key);

        public override string ToString() => Key;
        #endregion
    }
}
