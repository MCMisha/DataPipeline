using DataPipeline.Tests.Processors;
using DataPipeline.Core.Interfaces;

namespace DataPipeline.Core.Builders;

public class ConditionalStepBuilder<T> : IConditionalStepBuilder<T>
    where T : class
{
    private readonly PipelineBuilder<T> _parent;
    private readonly Func<T, bool> _condition;

    public ConditionalStepBuilder(
        PipelineBuilder<T> parent,
        Func<T, bool> condition)
    {
        _parent = parent;
        _condition = condition;
    }

    public PipelineBuilder<T> Then(Func<T, T> transformation)
    {
        ArgumentNullException.ThrowIfNull(transformation);
        
        ConditionalProcessor<T> conditionalProcessor = new ConditionalProcessor<T>(_condition, transformation);
        _parent.AddProcessor(conditionalProcessor);
        return _parent;
    }
}