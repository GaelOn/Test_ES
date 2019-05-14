using Domain.Base.Event;

namespace Domain.Base
{
    public interface IEventDriven<TStreamId>
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
    }
}