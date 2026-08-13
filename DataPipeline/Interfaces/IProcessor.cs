namespace ConsoleAppDataPipeline.Interfaces;

public interface IProcessor<T> where T : class
{
    T Process(T input);
}