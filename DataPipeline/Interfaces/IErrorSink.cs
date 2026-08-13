using ConsoleAppDataPipeline.Mappers;

namespace ConsoleAppDataPipeline.Interfaces;

public interface IErrorSink
{
    public Task WriteAsync(IReadOnlyList<MappingError> errors, CancellationToken token = default);
}