using System;

namespace Domain.Mock.Implem
{
    public interface IProcessElement
    {
        DateTime Created { get; }
        string Name { get; }
        string RunningService { get; }
        DateTime Start { get; }
        ProcessElementState State { get; }
        DateTime Stop { get; }
    }
}