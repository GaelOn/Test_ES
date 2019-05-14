using System;

namespace Domain.Base.Mock.CommunicationQueue
{
    public interface IHandlerRegister
    {
        void RegisterHandler<T>(Action<T> handler);
    }
}
