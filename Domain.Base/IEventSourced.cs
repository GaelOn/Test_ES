using Domain.Base.Event;
using System.Collections.Generic;

namespace Domain.Base
{

    public interface IEventSourced<TStreamId>
    {
        /// <summary>
        /// Id of the stream into all the event into the store. Same as the AggregateId if we are in case of an aggregate object.
        /// </summary>
        TStreamId StreamId { get; }
        /// <summary>
        /// Process an event into the object
        /// </summary>
        /// <param name="evt">the event to be processed.</param>
        /// <param name="version">the version number of the event.</param>
        void ProcessEvent(IDomainEvent<TStreamId> evt, long version);
        /// <summary>
        /// Version before commiting.
        /// </summary>
        long Version { get; }
        /// <summary>
        /// Ongoing version number due to actual processing between two commit. It's also the version of event.
        /// </summary>
        long CurrentVersion { get; }
        /// <summary>
        /// Store all uncommited event.
        /// </summary>
        IEnumerable<IDomainEvent<TStreamId>> UncommittedEvents { get; }
        /// <summary>
        /// TO clear the store of event.
        /// </summary>
        void ClearUncommittedEvents();
    }
}
