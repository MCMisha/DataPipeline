using ConsoleAppDataPipeline.Interfaces;

namespace ConsoleAppDataPipeline.Processors;

public class TransformationProcessor<T> : IProcessor<T>
    where T : class
{
    private readonly Func<T, T> _transformation;

    public TransformationProcessor(Func<T, T> transformation)
    {
        _transformation = transformation;
    }

    public T Process(T input)
    {
        return _transformation(input);
    }
}