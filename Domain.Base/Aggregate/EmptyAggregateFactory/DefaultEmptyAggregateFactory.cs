using System;
using System.Reflection;

namespace Domain.Base.Aggregate
{
    public class DefaultEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> : IEmptyAggregateFactory<TAggregate, TAggregateId, TEntityId> 
        where TAggregate : AggregateBase<TAggregateId, TEntityId>, new()
    {
        private readonly ConstructorInfo _constructor;

        public TAggregate GetEmptyAggregate() => _constructor.Invoke(new object[0]) as TAggregate;

        public DefaultEmptyAggregateFactory()
        {
            _constructor = typeof(TAggregate).GetConstructor(BindingFlags.Instance |
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public,
                                                             null, new Type[0], new ParameterModifier[0]);
        }
    }
}
