using DataPipeline.Core.Models;

namespace DataPipeline.Core.Interfaces;

public interface IDataSource<T> where T : class
{
    IAsyncEnumerable<MappingResult<T>> ReadAsync(
        Stream stream,
        IRecordMapper<T> mapper,
        CancellationToken cancellationToken = default);
}