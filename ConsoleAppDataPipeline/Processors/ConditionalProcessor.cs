using ConsoleAppDataPipeline.Interfaces;

namespace ConsoleAppDataPipeline.Processors;

public class ConditionalProcessor<T> : IProcessor<T> where T : class
{
    private readonly Func<T, bool> _condition;
    private readonly Func<T, T> _transformation;

    public ConditionalProcessor(Func<T, bool> condition, Func<T, T> transformation)
    {
        _condition = condition;
        _transformation = transformation;
    }
    
    public T Process(T input)
    {
        T output = input;
        if (_condition(input))
        {
            output = _transformation(output);
        }
        return output;
    }
}