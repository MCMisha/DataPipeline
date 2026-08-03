namespace ConsoleAppDataPipeline.Mappers;

public sealed record MappingError(
    string? Field,
    string Message,
    string? RawValue = null);