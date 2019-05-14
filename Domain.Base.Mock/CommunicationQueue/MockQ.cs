using System.Collections.Generic;
using System.Linq;

namespace Domain.Base.Mock.CommunicationQueue
{
    public class MockQ : ICommunicationQueue
    {
        private readonly List<Exchange> _exchanges = new List<Exchange>(10);

        public IMessagePublisher GetMessagePublisher(string exchangeName) => GetExchange(exchangeName).GetMessagePublisher();

        public IHandlerRegister GetHandlerRegister(string exchangeName) => GetExchange(exchangeName).GetHandlerRegister();

        private Exchange GetExchange(string exchangeName)
        {
            Exchange exch;
            exch = _exchanges.FirstOrDefault(exchange => exchange.ExchangeName == exchangeName);
            if (exch == null)
            {
                exch = CreateExchange(exchangeName);
            }
            return exch;
        }

        private Exchange CreateExchange(string exchangeName)
        {
            var exch = new Exchange(exchangeName);
            _exchanges.Add(exch);
            return exch;
        }
    }
}
