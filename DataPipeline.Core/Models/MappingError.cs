namespace DataPipeline.Core.Models;

public sealed record MappingError(
    string? Field,
    string Message,
    string? RawValue = null);