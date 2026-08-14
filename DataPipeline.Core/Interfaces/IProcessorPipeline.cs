namespace DataPipeline.Core.Interfaces;

public interface IProcessorPipeline<T>
{
    public T Process(T input);
}