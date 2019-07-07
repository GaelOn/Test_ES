using System;
using Domain.Mock.Implem;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;

namespace Domain.Base.Test.TestHelper
{
    public static class Scenario
    {
        public static InputAggregate Start_Process(ParamScenarioTest paramScenarioTest, Func<InputAggregate> aggregateFactory)
        {
            var aggregate = aggregateFactory();
            var processElemCreation = new FirstSubProcess(paramScenarioTest.ProcessName,
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

        public static Action Should_Throw_Due_To_Bad_StreamId(ParamScenarioTest paramScenarioTest, Func<InputAggregate> aggregateFactory)
        {
            var aggregate = aggregateFactory();
            var processElemCreation = new FirstSubProcess(paramScenarioTest.ProcessName,
                                                         paramScenarioTest.ExpectedProcessId,
                                                         paramScenarioTest.ExpectedDateCreated);
            aggregate.RaiseEvent(new InputAggregateCreated(paramScenarioTest.ExpectedStreamId + 1));
            Action shouldThrow = () => aggregate.RaiseEvent(new ProcessElementEntityCreated(paramScenarioTest.ExpectedStreamId, processElemCreation));
            return shouldThrow;
        }

        public static ParamScenarioTest GetArg(int argNumber = 0)
        {
            switch (argNumber)
            {
                case 0:
                    return new ParamScenarioTest("Test", 1, 10, "TestService", DateTime.Now, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(5), ProcessElementState.Ended);

                case 1:
                    return new ParamScenarioTest("Test1", 1, 10, "TestService", DateTime.Now, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(5), ProcessElementState.Ended);

                case 2:
                    return new ParamScenarioTest("Test2", 1, 11, "TestService", DateTime.Now.AddSeconds(30), DateTime.Now.AddMinutes(2), DateTime.Now.AddMinutes(6), ProcessElementState.Ended);

                default:
                    return null;
            }
        }
    }
}