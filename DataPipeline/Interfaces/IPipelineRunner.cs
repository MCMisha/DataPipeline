namespace ConsoleAppDataPipeline.Interfaces;

public interface IPipelineRunner<T>
{
    Task RunAsync(Stream stream, CancellationToken cancellationToken = default);
    string GetStatistics();
}