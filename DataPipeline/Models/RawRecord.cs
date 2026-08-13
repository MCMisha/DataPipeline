namespace ConsoleAppDataPipeline.Models;

public sealed class RawRecord
{
    private readonly IReadOnlyDictionary<string, string?> _values;

    public int RowNumber { get; }

    public RawRecord(
        int rowNumber,
        IReadOnlyDictionary<string, string?> values)
    {
        RowNumber = rowNumber;
        _values = values;
    }

    public string? GetValueOrDefault(string columnName)
    {
        return _values.GetValueOrDefault(columnName);
    }
}