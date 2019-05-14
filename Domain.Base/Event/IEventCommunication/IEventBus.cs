using System.Threading.Tasks;

namespace Domain.Base.Event.IEventCommunication
{
    /// <summary>
    /// Publish only
    /// </summary>
    public interface IEventBus
    {
        void PublishEvent<T, TAggregateId>(T evt) where T : IDomainEvent<TAggregateId>;
        Task PublishEventAsync<T, TAggregateId>(T evt) where T : IDomainEvent<TAggregateId>;
    }
}
