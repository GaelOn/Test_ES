using Domain.Base.Event;
using System;

namespace Domain.Base.Aggregate
{
    public interface IAggregateEventLifeCycleMedium<TStreamId>
    {
        void AddUncommitedEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent<TStreamId>;
        bool ExistUncommitedEvent(Func<IDomainEvent<TStreamId>,bool> existCriteria);
        long GetVersion();
        void PrepareEvent<TEvent>(TEvent evt) where TEvent : IDomainEvent<TStreamId>;
        bool TryCommitEventVersion(long version);
    }
}