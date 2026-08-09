using System.Text;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;

namespace ConsoleAppDataPipeline.Tests;

public class CsvDataSourceTests
{
    private CsvDataSource<TestRecord> _dataSource = null!;

    [SetUp]
    public void Setup()
    {
        _dataSource = new CsvDataSource<TestRecord>();
    }

    [Test]
    public async Task ReadAsync_ValidCsv_ReturnsAllRecords()
    {
        const string csv =
            """
            Name,Age
            Misha,26
            John,31
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].IsSuccess, Is.True);
            Assert.That(result[1].IsSuccess, Is.True);

            Assert.That(result[0].Value!.Name, Is.EqualTo("Misha"));
            Assert.That(result[0].Value!.Age, Is.EqualTo(26));

            Assert.That(result[1].Value!.Name, Is.EqualTo("John"));
            Assert.That(result[1].Value!.Age, Is.EqualTo(31));
        });
    }

    [Test]
    public async Task ReadAsync_EmptyFile_ReturnsEmptyCollection()
    {
        var mapper = new TestRecordMapper();

        var result = await ReadAsync(string.Empty, mapper);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ReadAsync_HeaderOnly_ReturnsEmptyCollection()
    {
        const string csv = "Name,Age";

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ReadAsync_BlankLines_IgnoresBlankLines()
    {
        const string csv =
            """
            Name,Age

            Misha,26

            John,31

            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(r => r.IsSuccess), Is.True);
        });
    }

    [Test]
    public async Task ReadAsync_RowHasTooFewColumns_ReturnsFailure()
    {
        const string csv =
            """
            Name,Age
            Misha
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].Errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task ReadAsync_RowHasTooManyColumns_ReturnsFailure()
    {
        const string csv =
            """
            Name,Age
            Misha,26,Gdansk
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].Errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task ReadAsync_EmptyHeader_ReturnsFailure()
    {
        const string csv =
            """

            Misha,26
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].RowNumber, Is.EqualTo(1));
            Assert.That(
                result[0].Errors[0].Message,
                Is.EqualTo("CSV header contains an empty column name"));
        });
    }

    [Test]
    public async Task ReadAsync_HeaderContainsEmptyHeader_ReturnsFailure()
    {
        const string csv =
            """
            Name,
            Misha,26
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].RowNumber, Is.EqualTo(1));
            Assert.That(
                result[0].Errors[0].Message,
                Is.EqualTo("CSV header contains an empty column name"));
        });
    }
    
    [Test]
    public async Task ReadAsync_DuplicateHeader_ReturnsFailure()
    {
        const string csv =
            """
            Name,Name,Age
            Misha,Yakushevich,26
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].RowNumber, Is.EqualTo(1));
            Assert.That(
                result[0].Errors[0].Message,
                Is.EqualTo("CSV header contains duplicate column names"));
        });
    }

    [Test]
    public async Task ReadAsync_DuplicateHeaderWithDifferentCase_ReturnsFailure()
    {
        const string csv =
            """
            Name,name,Age
            Misha,Yakushevich,26
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].RowNumber, Is.EqualTo(1));
            Assert.That(
                result[0].Errors[0].Message,
                Is.EqualTo("CSV header contains duplicate column names"));
        });
    }

    [Test]
    public async Task ReadAsync_MapperFailure_ReturnsFailure()
    {
        const string csv =
            """
            Name,Age
            Misha,abc
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsSuccess, Is.False);
            Assert.That(result[0].Errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task ReadAsync_MapperFailure_DoesNotStopReadingNextRows()
    {
        const string csv =
            """
            Name,Age
            Misha,26
            Invalid,abc
            John,31
            """;

        var mapper = new TestRecordMapper();

        var result = await ReadAsync(csv, mapper);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));

            Assert.That(result[0].IsSuccess, Is.True);
            Assert.That(result[1].IsSuccess, Is.False);
            Assert.That(result[2].IsSuccess, Is.True);

            Assert.That(result[0].Value!.Name, Is.EqualTo("Misha"));
            Assert.That(result[2].Value!.Name, Is.EqualTo("John"));
        });
    }

    [Test]
    public async Task ReadAsync_ValidRow_PassesCorrectValuesToMapper()
    {
        const string csv =
            """
            Name,Age
            Misha,26
            """;

        var mapper = new CapturingMapper();

        await ReadAsync(csv, mapper);

        Assert.That(mapper.ReceivedRecord, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(
                mapper.ReceivedRecord!.GetValueOrDefault("Name"),
                Is.EqualTo("Misha"));

            Assert.That(
                mapper.ReceivedRecord.GetValueOrDefault("Age"),
                Is.EqualTo("26"));
        });
    }

    private async Task<IReadOnlyList<MappingResult<TestRecord>>> ReadAsync(
        string csv,
        IRecordMapper<TestRecord> mapper)
    {
        await using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        var results = new List<MappingResult<TestRecord>>();

        await foreach (var result in _dataSource.ReadAsync(stream, mapper))
        {
            results.Add(result);
        }

        return results;
    }

    private sealed class TestRecord
    {
        public string Name { get; init; } = string.Empty;

        public int Age { get; init; }
    }

    private sealed class TestRecordMapper : IRecordMapper<TestRecord>
    {
        public MappingResult<TestRecord> Map(RawRecord record)
        {
            if (!int.TryParse(record.GetValueOrDefault("Age"), out var age))
            {
                return MappingResult<TestRecord>.Failure(
                    record.RowNumber,
                    new MappingError(
                        "Age",
                        "Age must be a valid integer.",
                        record.GetValueOrDefault("Age")));
            }

            return MappingResult<TestRecord>.Success(
                new TestRecord
                {
                    Name = record.GetValueOrDefault("Name")!,
                    Age = age
                });
        }
    }

    private sealed class CapturingMapper : IRecordMapper<TestRecord>
    {
        public RawRecord? ReceivedRecord { get; private set; }

        public MappingResult<TestRecord> Map(RawRecord record)
        {
            ReceivedRecord = record;

            return MappingResult<TestRecord>.Success(
                new TestRecord
                {
                    Name = record.GetValueOrDefault("Name")!,
                    Age = int.Parse(record.GetValueOrDefault("Age")!)
                });
        }
    }
}