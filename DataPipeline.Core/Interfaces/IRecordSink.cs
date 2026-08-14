namespace DataPipeline.Core.Interfaces;

public interface IRecordSink<T>
{
    Task WriteAsync(T record, CancellationToken cancellationToken = default);
}