using System;
using Domain.Base.Mock.CommunicationQueue;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    public class MessageProcessor
    {
        readonly IHandlerRegister _register;
        int _count;

        public int Count { get { return _count; } }

        public MessageProcessor(IHandlerRegister register) => _register = register;

        public void RegisterHandle<T>(Action<T> handle) => _register.RegisterHandler(handle);

        public void RegisterMessageToBeCounted<T>() => _register.RegisterHandler<T>(Increment);

        private void Increment<T>(T passiveEvt) => _count++;
    }
}
