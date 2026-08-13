using ConsoleAppDataPipeline.Mappers;

namespace ConsoleAppDataPipeline.Interfaces;

public interface IDataSource<T> where T : class
{
    IAsyncEnumerable<MappingResult<T>> ReadAsync(
        Stream stream,
        IRecordMapper<T> mapper,
        CancellationToken cancellationToken = default);
}