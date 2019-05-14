namespace Domain.Base.Mock.CommunicationQueue
{
    public interface ICommunicationQueue
    {
        IHandlerRegister GetHandlerRegister(string exchangeName);
        IMessagePublisher GetMessagePublisher(string exchangeName);
    }
}