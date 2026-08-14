using DataPipeline.Core.Models;

namespace DataPipeline.Core.Interfaces;

public interface IErrorSink
{
    public Task WriteAsync(IReadOnlyList<MappingError> errors, CancellationToken token = default);
}