using System;
using System.Threading.Tasks;

namespace Domain.Base.Event.IEventCommunication
{
    public interface IEventFeedSubscriber
    {
        void Subscribe<T, TAggregateId>(Action<T> handler) where T : IDomainEvent<TAggregateId>;
        void Subscribe<T, TAggregateId>(Func<T, Task> handler) where T : IDomainEvent<TAggregateId>;
    }
}
