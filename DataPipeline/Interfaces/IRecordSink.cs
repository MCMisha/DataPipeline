namespace ConsoleAppDataPipeline.Interfaces;

public interface IRecordSink<T>
{
    Task WriteAsync(T record, CancellationToken cancellationToken = default);
}