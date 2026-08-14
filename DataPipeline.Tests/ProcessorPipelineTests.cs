using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Runners;

namespace DataPipeline.Tests;

[TestFixture]
public class ProcessorPipelineTests
{
    [Test]
    public void Process_EmptyPipeline_ReturnsOriginalObject()
    {
        var input = new TestRecord(10);

        var pipeline = new ProcessorPipeline<TestRecord>(
            Array.Empty<IProcessor<TestRecord>>());

        var result = pipeline.Process(input);

        Assert.That(result, Is.SameAs(input));
    }

    [Test]
    public void Process_OneProcessor_ReturnsProcessedObject()
    {
        var input = new TestRecord(10);

        var pipeline = new ProcessorPipeline<TestRecord>(
        [
            new AddProcessor(5)
        ]);

        var result = pipeline.Process(input);

        Assert.That(result.Value, Is.EqualTo(15));
    }

    [Test]
    public void Process_MultipleProcessors_PassesResultToNextProcessor()
    {
        var input = new TestRecord(2);

        var pipeline = new ProcessorPipeline<TestRecord>(
        [
            new AddProcessor(1),
            new MultiplyProcessor(2)
        ]);

        var result = pipeline.Process(input);

        Assert.That(result.Value, Is.EqualTo(6));
    }

    private sealed record TestRecord(int Value);

    private sealed class AddProcessor(int value) : IProcessor<TestRecord>
    {
        public TestRecord Process(TestRecord input)
        {
            return new TestRecord(Value: input.Value + value);
        }
    }

    private sealed class MultiplyProcessor(int value) : IProcessor<TestRecord>
    {
        public TestRecord Process(TestRecord input)
        {
            return new TestRecord(Value: input.Value * value);
        }
    }
}