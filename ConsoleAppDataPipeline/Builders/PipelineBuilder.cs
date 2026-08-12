using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Processors;
using ConsoleAppDataPipeline.Runners;

namespace ConsoleAppDataPipeline.Builders;

public class PipelineBuilder<T> where T : class
{
    private readonly List<IProcessor<T>> _processors = [];
    public PipelineBuilder<T> AddProcessor(IProcessor<T> processor)
    {
        ArgumentNullException.ThrowIfNull(processor);
        
        _processors.Add(processor);
        return this;
    }

    public PipelineBuilder<T> Transform(Func<T, T> transformation)
    {
        ArgumentNullException.ThrowIfNull(transformation);
        
        _processors.Add(new TransformationProcessor<T>(transformation));
        return this;
    }

    public IConditionalStepBuilder<T> If(Func<T, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        return new ConditionalStepBuilder<T>(this, condition);
    }

    public IProcessorPipeline<T> Build()
    {
        return new ProcessorPipeline<T>(_processors.ToArray());
    }
}