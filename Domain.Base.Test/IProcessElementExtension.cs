using Domain.Mock.Implem;
using FluentAssertions;
using Domain.Base.Test;

namespace Domain.Base.AggregateBase.Test
{
    public static class IProcessElementExtension
    {
        public static void ShouldBeAsExpected(this IProcessElement processElement, ParamScenarioTest param)
        {
            processElement.Should().NotBeNull();
            processElement.RunningService.Should().Be(param.ExpectedRunningService);
            processElement.Start.Should().Be(param.ExpectedDateStarted);
            processElement.Stop.Should().Be(param.ExpectedDateStoped);
            processElement.State.Should().Be(param.ExpectedState);
        }

    }
}
