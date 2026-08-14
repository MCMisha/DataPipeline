using DataPipeline.Core.Builders;

namespace DataPipeline.Core.Interfaces;

public interface IConditionalStepBuilder<T> where T : class
{
    PipelineBuilder<T> Then(Func<T, T> transformation);
}