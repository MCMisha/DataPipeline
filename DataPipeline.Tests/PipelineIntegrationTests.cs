using System.Text;
using System.Text.Json;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Models;
using DataPipeline.Core.Runners;
using DataPipeline.Core.Sinks;
using DataPipeline.Csv;

namespace DataPipeline.Tests;

[TestFixture]
public class PipelineIntegrationTests
{
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(),
            $"DataPipelineTests_{Guid.NewGuid()}");

        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public async Task RunAsync_CsvToJson_WritesValidRecordsAndCollectsErrors()
    {
        const string csv =
            """
            Name,Email,Age,Gender
            Misha,misha@example.com,26,M
            Invalid,invalid@example.com,abc,C
            Anna,anna@example.com,31,F
            """;

        await using var inputStream = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        var outputPath = Path.Combine(
            _tempDirectory,
            "users.json");

        var dataSource = new CsvDataSource<User>();
        var mapper = new UserMapper();
        var recordSink = new JsonRecordSink<User>(outputPath);
        var errorSink = new CollectingErrorSink();

        var runner = new PipelineRunner<User>(
            dataSource,
            mapper,
            recordSink,
            errorSink);

        await runner.RunAsync(inputStream);

        await recordSink.DisposeAsync();

        var json = await File.ReadAllTextAsync(outputPath);

        var users = JsonSerializer.Deserialize<List<User>>(json);

        Assert.Multiple(() =>
        {
            Assert.That(users, Has.Count.EqualTo(2));

            Assert.That(
                users![0],
                Is.EqualTo(
                    new User(
                        "Misha",
                        "misha@example.com",
                        26,
                        'M')));

            Assert.That(
                users[1],
                Is.EqualTo(
                    new User(
                        "Anna",
                        "anna@example.com",
                        31,
                        'F')));

            Assert.That(errorSink.Errors, Has.Count.EqualTo(2));
            Assert.That(errorSink.Errors[0].Field, Is.EqualTo("Age"));
            Assert.That(
                errorSink.Errors[0].RawValue,
                Is.EqualTo("abc"));
        });
    }

    private sealed class CollectingErrorSink : IErrorSink
    {
        public List<MappingError> Errors { get; } = [];

        public Task WriteAsync(
            IReadOnlyList<MappingError> errors,
            CancellationToken token = default)
        {
            Errors.AddRange(errors);

            return Task.CompletedTask;
        }
    }
}