using System;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;

namespace Domain.Base.Test
{
    public static class Scenario
    {
        public static InputAggregate Start_Process(ParamScenarioTest paramScenarioTest, Func<InputAggregate> aggregateFactory)
        {
            var aggregate = aggregateFactory();
            var processElemCreation = new ProcessElement(paramScenarioTest.ProcessName,
                                                         paramScenarioTest.ExpectedProcessId,
                                                         paramScenarioTest.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(paramScenarioTest.ExpectedStreamId));
            aggregate.RaiseEvent(new ProcessElementEntityCreated(paramScenarioTest.ExpectedStreamId,
                                                                 processElemCreation));
            aggregate.RaiseEvent(new ProcessElemStarted(paramScenarioTest.ExpectedStreamId,
                                                        paramScenarioTest.ExpectedProcessId,
                                                        paramScenarioTest.ExpectedRunningService,
                                                        paramScenarioTest.ExpectedDateStarted));
            return aggregate;
        }
        public static InputAggregate Should_Throw_Due_To_Bad_StreamId(ParamScenarioTest paramScenarioTest, Func<InputAggregate> aggregateFactory)
        {
            var aggregate = aggregateFactory();
            var processElemCreation = new ProcessElement(paramScenarioTest.ProcessName,
                                                         paramScenarioTest.ExpectedProcessId,
                                                         paramScenarioTest.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(paramScenarioTest.ExpectedStreamId + 1));
            //aggregate.RaiseEvent(new ProcessElementEntityCreated(paramScenarioTest.ExpectedStreamId,
            //                                                     processElemCreation));
            return aggregate;
        }

        public static ParamScenarioTest GetArg()
        {
            return new ParamScenarioTest("Test", 1, 10, "TestService", DateTime.Now, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(5));
        }
    }
}
