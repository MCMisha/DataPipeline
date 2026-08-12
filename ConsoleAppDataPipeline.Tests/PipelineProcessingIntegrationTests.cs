using System.Text;
using System.Text.Json;
using ConsoleAppDataPipeline.Builders;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;
using ConsoleAppDataPipeline.Runners;
using ConsoleAppDataPipeline.Sinks;

namespace ConsoleAppDataPipeline.Tests;

[TestFixture]
public class PipelineProcessingIntegrationTests
{
    [Test]
    public async Task RunAsync_WithProcessingPipeline_WritesTransformedUserToJson()
    {
        const string csv =
            """
            Name,Email,Age,Gender
            Anna,anna@example.com,18,F
            """;

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"DataPipeline_{Guid.NewGuid()}.json");

        try
        {
            await using var inputStream = new MemoryStream(
                Encoding.UTF8.GetBytes(csv));

            var dataSource = new CsvDataSource<User>();
            var mapper = new UserMapper();
            var errorSink = new CollectingErrorSink();

            var processingPipeline = new PipelineBuilder<User>()
                .If(user => user.Age >= 18)
                .Then(user => user with
                {
                    Name = $"Adult {user.Name}"
                })
                .Transform(user => user with
                {
                    Age = user.Age + 1
                })
                .Build();

            await using (var recordSink =
                         new JsonRecordSink<User>(outputPath))
            {
                var runner = new PipelineRunner<User>(
                    dataSource,
                    mapper,
                    recordSink,
                    errorSink,
                    processingPipeline);

                await runner.RunAsync(inputStream);
            }

            var json = await File.ReadAllTextAsync(outputPath);

            var users = JsonSerializer.Deserialize<List<User>>(json);

            Assert.Multiple(() =>
            {
                Assert.That(users, Has.Count.EqualTo(1));

                Assert.That(
                    users![0],
                    Is.EqualTo(
                        new User(
                            "Adult Anna",
                            "anna@example.com",
                            19,
                            'F')));

                Assert.That(errorSink.Errors, Is.Empty);
            });
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
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