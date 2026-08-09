using ConsoleAppDataPipeline.Interfaces;

namespace ConsoleAppDataPipeline.Runners;

public class PipelineRunner<T> : IPipelineRunner<T> where T : class
{
    private readonly IDataSource<T> _dataSource;
    private readonly IRecordMapper<T> _mapper;
    private readonly IRecordSink<T> _recordSink;
    private readonly IErrorSink _errorSink;
    private PipelineRunnerStatistics _statistics;

    public PipelineRunner(
        IDataSource<T> dataSource,
        IRecordMapper<T> mapper,
        IRecordSink<T> recordSink,
        IErrorSink errorSink)
    {
        _dataSource = dataSource;
        _mapper = mapper;
        _recordSink = recordSink;
        _errorSink = errorSink;
        _statistics = new PipelineRunnerStatistics();
    }

    public async Task RunAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        DateTime startTime = DateTime.Now;
        await foreach (var result in _dataSource.ReadAsync(
                           stream,
                           _mapper,
                           cancellationToken))
        {
            if (result.IsSuccess)
            {
                await _recordSink.WriteAsync(
                    result.Value!,
                    cancellationToken);
                _statistics.SuccessfulRecords++;
            }
            else
            {
                await _errorSink.WriteAsync(
                    result.Errors, cancellationToken);
                _statistics.FailedRecords++;
            }
            _statistics.TotalRecords++;
        }

        _statistics.Duration = DateTime.Now.Subtract(startTime);
    }
    
    public string GetStatistics() => _statistics.ToString();
}