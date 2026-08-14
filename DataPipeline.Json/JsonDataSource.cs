using System.Runtime.CompilerServices;
using System.Text.Json;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Models;

namespace DataPipeline.Json;

public class JsonDataSource<T> : IDataSource<T> where T: class
{
    public async IAsyncEnumerable<MappingResult<T>> ReadAsync(
        Stream stream,
        IRecordMapper<T> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(mapper);

        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "The root JSON element must be an array.");
        }

        var rowNumber = 1;

        foreach (var element in document.RootElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (element.ValueKind != JsonValueKind.Object)
            {
                yield return MappingResult<T>.Failure(rowNumber,
                    new MappingError(
                        "undefined",
                        "The JSON array element must be an object."));

                rowNumber++;
                continue;
            }

            var values = new Dictionary<string, string?>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var property in element.EnumerateObject())
            {
                values[property.Name] = ConvertToString(property.Value);
            }

            var rawRecord = new RawRecord(rowNumber, values);

            yield return mapper.Map(rawRecord);

            rowNumber++;
        }
    }

    private static string? ConvertToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

}