namespace DataPipeline.Core.Interfaces;

public interface IPipelineRunner<T>
{
    Task RunAsync(Stream stream, CancellationToken cancellationToken = default);
    string GetStatistics();
}