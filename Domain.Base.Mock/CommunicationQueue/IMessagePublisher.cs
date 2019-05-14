namespace Domain.Base.Mock.CommunicationQueue
{
    public interface IMessagePublisher
    {
        void Send<T>(T msg);
    }
}
