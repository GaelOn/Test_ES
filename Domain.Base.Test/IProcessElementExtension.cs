using Domain.Mock.Implem;
using FluentAssertions;
using Domain.Base.Test;

namespace Domain.Base.AggregateBase.Test
{
    public static class IProcessElementExtension
    {
        public static void ShouldBeAsExpected(this IProcessElement pe, ParamScenarioTest param)
        {
            pe.Should().NotBeNull();
            pe.RunningService.Should().Be(param.ExpectedRunningService);
            pe.Start.Should().Be(param.ExpectedDateStarted);
            pe.Stop.Should().Be(param.ExpectedDateStoped);
            pe.State.Should().Be(param.ExpectedState);
        }

    }
}
