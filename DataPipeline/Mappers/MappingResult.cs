namespace ConsoleAppDataPipeline.Mappers;

public sealed record MappingResult<T>
{
    public T? Value { get; }
    public IReadOnlyList<MappingError> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public int? RowNumber { get; }
    

    private MappingResult(T? value, IReadOnlyList<MappingError> errors, int? rowNumber)
    {
        Value = value;
        Errors = errors;
        RowNumber = rowNumber;
    }

    public static MappingResult<T> Success(T value)
    {
        return new MappingResult<T>(value, [], null);
    }

    public static MappingResult<T> Failure(int rowNumber, params MappingError[] errors)
    {
        return new MappingResult<T>(default, errors, rowNumber);
    }
}