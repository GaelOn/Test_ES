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

        public EventWrapper(IDomainEvent<TAggregateId> domainEvent) => DomainEvent = domainEvent;

        public string Key => ToString();
        public long EventNumber { get; private set; }
        public IDomainEvent<TAggregateId> DomainEvent { get; }

        #region Object override and IEquatable<EventWrapper<TAggregateId>>
        public override bool Equals(object obj)
        {
            EventWrapper<TAggregateId> castedObj;
            bool IsOfTypeEventWrapper()
            {
                castedObj = obj as EventWrapper<TAggregateId>;
                return castedObj != null;
            }
            return obj != null &&
                    IsOfTypeEventWrapper() &&
                    Equals(castedObj);
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public bool Equals(EventWrapper<TAggregateId> other) => ToString().Equals(other.ToString());

        public override string ToString() => $"{StreamId}:{Version}";
        #endregion
    }
}
