using DataPipeline.Core.Interfaces;

namespace DataPipeline.Core.Runners;

public class PipelineRunner<T> : IPipelineRunner<T> where T : class
{
    private readonly IDataSource<T> _dataSource;
    private readonly IRecordMapper<T> _mapper;
    private readonly IRecordSink<T> _recordSink;
    private readonly IErrorSink _errorSink;
    private readonly IProcessorPipeline<T>? _processorPipeline;
    private PipelineRunnerStatistics _statistics;

    public PipelineRunner(
        IDataSource<T> dataSource,
        IRecordMapper<T> mapper,
        IRecordSink<T> recordSink,
        IErrorSink errorSink,
        IProcessorPipeline<T>? pipeline = null)
    {
        _dataSource = dataSource;
        _mapper = mapper;
        _recordSink = recordSink;
        _errorSink = errorSink;
        _processorPipeline = pipeline;
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
                var processedValue = _processorPipeline?.Process(result.Value!);
                await _recordSink.WriteAsync(
                    processedValue ?? result.Value!,
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