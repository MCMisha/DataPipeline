namespace DataPipeline.Core.Runners;

public sealed class PipelineRunnerStatistics
{
    public int TotalRecords { get; set; }

    public int SuccessfulRecords { get; set; }

    public int FailedRecords { get; set; }

    public TimeSpan Duration { get; set; }

    public override string ToString() =>
        $"Total: {TotalRecords}, Successful: {SuccessfulRecords}, Failed: {FailedRecords}, Duration: {Duration}";
}