using Domain.Base.Mock.CommunicationQueue;
using System.Threading;

namespace ToyConsole.TestQ
{
    public class MessageProducer
    {
        IMessagePublisher _publisher;

        public MessageProducer(IMessagePublisher publisher) => _publisher = publisher;

        public void Run()
        {
            for (int it = 5; it > -1; it--)
            {
                _publisher.Send(new CountMessage(it));
                Thread.Sleep(1000);
            }
            _publisher.Send(new EndMessage());
        }
    }
}
