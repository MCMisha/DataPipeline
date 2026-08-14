using System.Runtime.CompilerServices;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Models;

namespace DataPipeline.Csv;

public sealed class CsvDataSource<T> : IDataSource<T> where T : class
{
    public async IAsyncEnumerable<MappingResult<T>> ReadAsync(
        Stream stream,
        IRecordMapper<T> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (StreamReader reader = new StreamReader(stream))
        {
            string? headerLine = await reader.ReadLineAsync(cancellationToken);

            if (headerLine is null)
            {
                yield break;
            }

            string[] keys = headerLine
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();

            var rowNumber = 1;
            if (keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != keys.Length)
            {
                yield return MappingResult<T>.Failure(rowNumber,
                    new MappingError(
                        null,
                        "CSV header contains duplicate column names",
                        headerLine));
                yield break;
            }

            if (keys.Any(string.IsNullOrWhiteSpace))
            {
                yield return
                    MappingResult<T>.Failure(
                        rowNumber,
                        new MappingError(
                            null,
                            "CSV header contains an empty column name",
                            headerLine))
                    ;
                yield break;
            }

            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                rowNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] values = line.Split(",");
                if (values.Length != keys.Length)
                {
                    yield return MappingResult<T>.Failure(rowNumber, new MappingError(null,
                        $"Expected {keys.Length} columns, but found {values.Length}", line));
                    continue;
                }

                var keyValues = new Dictionary<string, string?>();
                for (int j = 0; j < keys.Length; j++)
                {
                    string key = keys[j];
                    string value = values[j].Trim();

                    keyValues[key] = value;
                }

                yield return mapper.Map(new RawRecord(rowNumber, keyValues));
            }
        }
    }
}