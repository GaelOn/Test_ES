using System;
using System.Collections.Generic;

namespace Domain.Base.Mock.CommunicationQueue
{
    class Publisher
    {
        private readonly List<Action<object>> _observer = new List<Action<object>>(20);
        private readonly Queue<object> _objects = new Queue<object>();
        private readonly Action Start;
        private readonly Action Stop;
        private bool _runnable;


        public Publisher(Action start, Action stop, Queue<object> msgs)
        {
            //NullGuard(() => start != null, "start");
            //NullGuard(() => stop  != null, "stop");
            Start = start;
            Stop = stop;
            _objects = msgs;
        }

        private void NullGuard(Func<bool> condition, string argName)
        {
            if (condition())
            {
                throw new ArgumentNullException("argName");
            }
        }

        public void Notify()
        {
            Start();
            while (_runnable && _objects.Count != 0)
            {
                _observer.ForEach(action => action(_objects.Dequeue()));
            }
            Stop();
        }

        public void RegisterHandler(Action<object> onMsg)
        {
            _runnable = true;
            _observer.Add(onMsg);
            if (_objects.Count != 0)
            {
                Notify();
            }
        }
    }
}
