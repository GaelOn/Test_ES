using System;
using System.Collections.Generic;

namespace Domain.Base.Mock.CommunicationQueue
{
    class Route
    {
        private Publisher _publisher;
        private RouteHandlerRegister _routeHandlerRegister;

        private volatile bool _isWorking = false;
        public string Name => Path?.ToString();
        public Path Path { get; }

        public Route(Path path, Queue<object> msgs)
        {
            Path = path;
            void start() => _isWorking = true;
            void stop()  => _isWorking = false;
            _publisher = new Publisher(start, stop, msgs);
            _routeHandlerRegister = new RouteHandlerRegister(this);
        }

        public IHandlerRegister HandlerRegister => _routeHandlerRegister;

        public void Poke()
        {
            if (!_isWorking)
            {
                _publisher.Notify();
            }
        }

        private class RouteHandlerRegister : IHandlerRegister
        {
            private readonly Route _route;

            public RouteHandlerRegister(Route route) => _route = route;

            public void RegisterHandler<T>(Action<T> handler) => _route._publisher.RegisterHandler(obj => handler((T)obj));
        }
    }
}
