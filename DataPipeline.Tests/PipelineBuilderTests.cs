using ConsoleAppDataPipeline.Builders;

namespace ConsoleAppDataPipeline.Tests;

[TestFixture]
public class PipelineBuilderTests
{
    [Test]
    public void Build_EmptyBuilder_ReturnsEmptyPipeline()
    {
        var builder = new PipelineBuilder<TestRecord>();
        var input = new TestRecord(10);

        var pipeline = builder.Build();

        var result = pipeline.Process(input);

        Assert.That(result, Is.SameAs(input));
    }

    [Test]
    public void Transform_AddsTransformationProcessor()
    {
        var builder = new PipelineBuilder<TestRecord>();

        var pipeline = builder
            .Transform(record =>
                new TestRecord(Value: record.Value + 5))
            .Build();

        var result = pipeline.Process(new TestRecord(10));

        Assert.That(result.Value, Is.EqualTo(15));
    }

    [Test]
    public void If_ConditionIsTrue_AppliesTransformation()
    {
        var builder = new PipelineBuilder<TestRecord>();

        var pipeline = builder
            .If(record => record.Value >= 10)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Build();

        var result = pipeline.Process(new TestRecord(10));

        Assert.That(result.Value, Is.EqualTo(20));
    }

    [Test]
    public void Build_CreatesIndependentPipeline()
    {
        var builder = new PipelineBuilder<TestRecord>();

        builder.Transform(record =>
            new TestRecord(Value: record.Value + 1));

        var pipeline = builder.Build();

        builder.Transform(record =>
            new TestRecord(Value: record.Value * 10));

        var result = pipeline.Process(new TestRecord(1));

        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public void If_ConditionIsFalse_DoesNotApplyTransformation()
    {
        var pipeline = new PipelineBuilder<TestRecord>()
            .If(record => record.Value >= 10)
            .Then(record => new TestRecord(Value: record.Value * 2))
            .Build();

        var input = new TestRecord(5);

        var result = pipeline.Process(input);

        Assert.That(result, Is.SameAs(input));
    }

    private sealed record TestRecord(int Value);
}