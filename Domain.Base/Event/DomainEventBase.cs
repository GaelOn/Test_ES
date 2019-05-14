using System;
using System.Collections.Generic;
using Domain.Base.Event;
using Domain.Base.Aggregate.AggregateException;

namespace Domain.Base
{
    public abstract class DomainEventBase<TStreamId> : IDomainEvent<TStreamId>, IEquatable<DomainEventBase<TStreamId>>
    {
        #region IDomainEvent<TStreamId>
        public Guid EventId => Guid.NewGuid();
        public TStreamId StreamId { get; }
        public long EventVersion { get; protected set; }

        public virtual void OfAggregate(IEventSourced<TStreamId> aggregate)
        {
            if (aggregate.StreamId.Equals(default(TStreamId)) || !aggregate.StreamId.Equals(StreamId))
            {
                throw AggregateIdNotMatchException.GetAggregateIdNotMatchExceptionFromAggregateAndEvent(aggregate, this);
            }
            EventVersion = aggregate.CurrentVersion;
        }
        #endregion

        #region ctor
        protected DomainEventBase(TStreamId streamId)
        {
            StreamId = streamId;
        }
        protected DomainEventBase(TStreamId streamId, long version) : this(streamId)
        {
            EventVersion = version;
        }
        #endregion

        #region IEquatable<DomainEventBase<TStreamId>> and Object
        public override bool Equals(object obj)
        {
            return Equals(obj as DomainEventBase<TStreamId>);
        }

        public bool Equals(DomainEventBase<TStreamId> other)
        {
            return other != null &&
                   EventId.Equals(other.EventId);
        }

        public override int GetHashCode()
        {
            return 290933282 + EqualityComparer<Guid>.Default.GetHashCode(EventId);
        }
        #endregion
    }
}
