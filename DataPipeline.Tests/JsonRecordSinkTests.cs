using System.Text.Json;
using DataPipeline.Core.Sinks;

namespace DataPipeline.Tests;

[TestFixture]
public class JsonRecordSinkTests
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
    public async Task DisposeAsync_WithNoRecords_WritesEmptyJsonArray()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.DisposeAsync();

        var json = await File.ReadAllTextAsync(filePath);

        Assert.That(json, Is.EqualTo("[]"));
    }

    [Test]
    public async Task WriteAsync_WithSingleRecord_WritesValidJson()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.WriteAsync(
            new User(
                "Misha",
                "misha@example.com",
                26,
                'M'));

        await sink.DisposeAsync();

        var json = await File.ReadAllTextAsync(filePath);

        Assert.DoesNotThrow(() => JsonDocument.Parse(json));

        var users = JsonSerializer.Deserialize<List<User>>(json);

        Assert.That(users, Has.Count.EqualTo(1));
        Assert.That(
            users![0],
            Is.EqualTo(
                new User(
                    "Misha",
                    "misha@example.com",
                    26,
                    'M')));
    }

    [Test]
    public async Task WriteAsync_WithMultipleRecords_WritesAllRecords()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.WriteAsync(
            new User(
                "Misha",
                "misha@example.com",
                26,
                'M'));

        await sink.WriteAsync(
            new User(
                "Anna",
                "anna@example.com",
                31,
                'F'));

        await sink.DisposeAsync();

        var json = await File.ReadAllTextAsync(filePath);

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
        });
    }

    [Test]
    public async Task Constructor_WithNestedDirectory_CreatesDirectory()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "nested",
            "output",
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.DisposeAsync();

        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public async Task WriteAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.DisposeAsync();

        Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await sink.WriteAsync(
                new User(
                    "Misha",
                    "misha@example.com",
                    26, 
                    'M')));
    }

    [Test]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        var filePath = Path.Combine(
            _tempDirectory,
            "users.json");

        var sink = new JsonRecordSink<User>(filePath);

        await sink.DisposeAsync();

        Assert.DoesNotThrowAsync(async () => await sink.DisposeAsync());
    }
}