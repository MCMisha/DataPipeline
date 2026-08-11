namespace ConsoleAppDataPipeline.Interfaces;

public interface IProcessorPipeline<T>
{
    public T Process(T input);
}