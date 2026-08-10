using System.Text;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;
using System.Text.Json;

namespace ConsoleAppDataPipeline.Tests;

[TestFixture]
public class JsonDataSourceTests
{
    private JsonDataSource<User> _dataSource = null!;
    private UserMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        _dataSource = new JsonDataSource<User>();
        _mapper = new UserMapper();
    }

    [Test]
    public async Task ReadAsync_WithTwoValidObjects_ReturnsTwoSuccessfulResults()
    {
        const string json = """
                            [
                              {
                                "Name": "Anna",
                                "Email": "anna@example.com",
                                "Age": 25,
                                "Gender": "F"
                              },
                              {
                                "Name": "John",
                                "Email": "john@example.com",
                                "Age": 31,
                                "Gender": "M"
                              }
                            ]
                            """;

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.All(result => result.IsSuccess), Is.True);

        Assert.That(results[0].Value, Is.EqualTo(
            new User("Anna", "anna@example.com", 25, 'F')));

        Assert.That(results[1].Value, Is.EqualTo(
            new User("John", "john@example.com", 31, 'M')));
    }

    [Test]
    public async Task ReadAsync_WithEmptyArray_ReturnsNoResults()
    {
        const string json = "[]";

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void ReadAsync_WithObjectAsRoot_ThrowsJsonException()
    {
        const string json = """
                            {
                              "Name": "Anna",
                              "Email": "anna@example.com",
                              "Age": 25
                            }
                            """;

        using var stream = CreateStream(json);

        var exception = Assert.ThrowsAsync<JsonException>(
            async () =>
            {
                await foreach (var _ in _dataSource.ReadAsync(stream, _mapper))
                {
                }
            });

        Assert.That(
            exception!.Message,
            Is.EqualTo("The root JSON element must be an array."));
    }

    [Test]
    public async Task ReadAsync_WithInvalidAge_ReturnsFailedMappingResult()
    {
        const string json = """
                            [
                              {
                                "Name": "Anna",
                                "Email": "anna@example.com",
                                "Age": "not-a-number",
                                "Gender": "F"
                              }
                            ]
                            """;

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Has.Count.EqualTo(1));

        var result = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.RowNumber, Is.EqualTo(1));
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });

        var error = result.Errors[0];

        Assert.Multiple(() =>
        {
            Assert.That(error.Field, Is.EqualTo("Age"));
            Assert.That(error.Message, Is.EqualTo("Age must be an integer"));
            Assert.That(error.RawValue, Is.EqualTo("not-a-number"));
        });
    }

    [Test]
    public async Task ReadAsync_WithNonObjectArrayElement_ReturnsFailedResult()
    {
        const string json = """
                            [
                              42
                            ]
                            """;

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Has.Count.EqualTo(1));

        var result = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.RowNumber, Is.EqualTo(1));
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task ReadAsync_WithMissingRequiredField_ReturnsMappingError()
    {
        const string json = """
                            [
                              {
                                "Email": "anna@example.com",
                                "Age": 25
                              }
                            ]
                            """;

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Has.Count.EqualTo(1));

        var result = results[0];

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(
            result.Errors.Any(error =>
                error.Field == "Name" &&
                error.Message == "Name is required"),
            Is.True);
    }

    [Test]
    public async Task ReadAsync_WithLowercasePropertyNames_MapsUserSuccessfully()
    {
        const string json = """
                            [
                              {
                                "name": "Anna",
                                "email": "anna@example.com",
                                "age": 25,
                                "gender": "F"
                              }
                            ]
                            """;

        await using var stream = CreateStream(json);

        var results = await ReadAllAsync(
            _dataSource.ReadAsync(stream, _mapper));

        Assert.That(results, Has.Count.EqualTo(1));

        var result = results[0];

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(
            result.Value,
            Is.EqualTo(new User("Anna", "anna@example.com", 25, 'F')));
    }
    
    [Test]
    public void ReadAsync_WithMalformedJson_ThrowsJsonException()
    {
        const string json =
            """
            [
              {
                "Name": "Anna",
                "Email": "anna@example.com",
                "Age": 25
            ]
            """;

        using var stream = CreateStream(json);

        Assert.ThrowsAsync(Is.InstanceOf<JsonException>(),
            async () =>
            {
                if (stream != null)
                    await foreach (var _ in _dataSource.ReadAsync(stream, _mapper))
                    {
                    }
            });
    }

    private static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    private static async Task<List<MappingResult<T>>> ReadAllAsync<T>(
        IAsyncEnumerable<MappingResult<T>> source)
    {
        var results = new List<MappingResult<T>>();

        await foreach (var result in source)
        {
            results.Add(result);
        }

        return results;
    }
}