using ConsoleAppDataPipeline.Builders;

namespace ConsoleAppDataPipeline.Interfaces;

public interface IConditionalStepBuilder<T> where T : class
{
    PipelineBuilder<T> Then(Func<T, T> transformation);
}