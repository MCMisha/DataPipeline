using DataPipeline.Core.Builders;

namespace DataPipeline.Tests;

[TestFixture]
public class PipelineBuilderDslTests
{
    [Test]
    public void If_ConditionIsTrue_ThenAppliesTransformation()
    {
        var pipeline = new PipelineBuilder<TestRecord>()
            .If(record => record.Value >= 10)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Build();

        var result = pipeline.Process(new TestRecord(10));

        Assert.That(result.Value, Is.EqualTo(20));
    }

    [Test]
    public void If_ConditionIsFalse_ThenDoesNotApplyTransformation()
    {
        var input = new TestRecord(5);

        var pipeline = new PipelineBuilder<TestRecord>()
            .If(record => record.Value >= 10)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Build();

        var result = pipeline.Process(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(input));
            Assert.That(result.Value, Is.EqualTo(5));
        });
    }

    [Test]
    public void Then_AllowsTransformToBeCalledAfterwards()
    {
        var pipeline = new PipelineBuilder<TestRecord>()
            .If(record => record.Value >= 10)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Transform(record => new TestRecord(Value: record.Value + 5))
            .Build();

        var result = pipeline.Process(new TestRecord(10));

        Assert.That(result.Value, Is.EqualTo(25));
    }

    [Test]
    public void MultipleIfThen_AreExecutedSequentially()
    {
        var pipeline = new PipelineBuilder<TestRecord>()
            .If(record => record.Value >= 2)
            .Then(record => new TestRecord(Value: record.Value + 3))
            .If(record => record.Value == 5)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Build();

        var result = pipeline.Process(new TestRecord(2));

        Assert.That(result.Value, Is.EqualTo(10));
    }

    [Test]
    public void TransformIfThenTransform_PreservesProcessorOrder()
    {
        var pipeline = new PipelineBuilder<TestRecord>()
            .Transform(record => new TestRecord(Value: record.Value + 1))
            .If(record => record.Value > 2)
            .Then(record => new TestRecord(Value: record.Value * 10))
            .Transform(record => new TestRecord(Value: record.Value - 4))
            .Build();

        var result = pipeline.Process(new TestRecord(2));

        Assert.That(result.Value, Is.EqualTo(26));
    }

    [Test]
    public void Build_CreatesPipelineIndependentFromLaterBuilderChanges()
    {
        var builder = new PipelineBuilder<TestRecord>();

        builder.Transform(record => new TestRecord(Value: record.Value + 1));

        var firstPipeline = builder.Build();

        builder
            .If(record => record.Value >= 2)
            .Then(record => new TestRecord(Value: record.Value * 10));

        var result = firstPipeline.Process(new TestRecord(1));

        Assert.That(result.Value, Is.EqualTo(2));
    }

    private sealed record TestRecord(int Value);
}