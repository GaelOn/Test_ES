using System;
using System.Collections.Generic;

namespace Domain.Base.Mock.CommunicationQueue
{
    class Exchange
    {
        private readonly Dictionary<Type, Queue<object>> _exchange = new Dictionary<Type, Queue<object>>(20);
        private readonly Dictionary<Type, Route> _routes = new Dictionary<Type, Route>(20);
        public string ExchangeName { get; }

        private class ExchangeMessagePublisher : IMessagePublisher
        {
            private readonly Exchange _exchange;

            public ExchangeMessagePublisher(Exchange exchange)
            {
                _exchange = exchange;
            }

            public void Send<T>(T msg)
            {
                _exchange.Send(msg);
            }
        }

        private class ExchangeHandlerRegister : IHandlerRegister
        {
            private readonly Exchange _exchange;

            public ExchangeHandlerRegister(Exchange exchange)
            {
                _exchange = exchange;
            }

            public void RegisterHandler<T>(Action<T> handler)
            {
                var type = typeof(T);
                if (!_exchange._routes.ContainsKey(type))
                {
                    _exchange.CreateRoute(type);
                }
                _exchange._routes[type].HandlerRegister.RegisterHandler(handler); ;
            }
        }

        public Exchange(string exchangeName) => ExchangeName = exchangeName;

        public IMessagePublisher GetMessagePublisher() => new ExchangeMessagePublisher(this);

        public IHandlerRegister GetHandlerRegister() => new ExchangeHandlerRegister(this);

        public void Send<T>(T msg)
        {
            AddToExchange(msg);
            if (_routes.ContainsKey(msg.GetType()))
            {
                _routes[msg.GetType()].Poke();
            }
        }

        private void AddToExchange<T>(T msg)
        {
            var type = msg.GetType();
            if (!_exchange.ContainsKey(type))
            {
                CreateRoute(type);
            }
            _exchange[type].Enqueue(msg);
        }

        private void CreateRoute(Type type)
        {
            var exq = new Queue<object>();
            _exchange.Add(type, exq);
            var path = new Path(ExchangeName, type.Name);
            var route = new Route(path, exq);
            _routes.Add(type, route);
        }
    }
}
