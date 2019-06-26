using System;

namespace Domain.Base.Event.EventHandler
{
    public class DomainEventHandler<T, TStreamId> : IDomainEventHandler<T, TStreamId> where T : class, IDomainEvent<TStreamId>
    {
        #region Private field

        private readonly Action<T> _concreteHandler;
        private readonly Action<IDomainEventHandler<TStreamId>, IDomainEvent<TStreamId>> _continuation;

        #endregion Private field

        #region ctor

        public DomainEventHandler(Action<T> concreteHandler, Action<IDomainEventHandler<TStreamId>, IDomainEvent<TStreamId>> continuation)
        {
            _concreteHandler = concreteHandler;
            _continuation = continuation;
        }

        #endregion ctor

        #region Implementation of IDomainEventHandler<TStreamId>

        public void Continue(IDomainEvent<TStreamId> evt) => _continuation(this as IDomainEventHandler<TStreamId>, evt);

        public void ProcessEvent(IDomainEvent<TStreamId> evt) => _concreteHandler(evt as T);

        #endregion Implementation of IDomainEventHandler<TStreamId>
    }
}