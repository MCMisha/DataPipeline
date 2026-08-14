using DataPipeline.Core.Interfaces;

namespace DataPipeline.Core.Runners;

public class ProcessorPipeline<T> : IProcessorPipeline<T> where T : class
{
    IEnumerable<IProcessor<T>> _processors;

    public ProcessorPipeline(IEnumerable<IProcessor<T>> processors)
    {
        _processors = processors;
    }

    public T Process(T input)
    {
        var current = input;

        foreach (var processor in _processors)
        {
            current = processor.Process(current);
        }
        return current;
    }
}