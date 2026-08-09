using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;

namespace ConsoleAppDataPipeline.Sinks;

public class UserConsoleErrorSink : IErrorSink
{
    public Task WriteAsync(IReadOnlyList<MappingError> errors, CancellationToken token = default)
    { 
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"[{DateTime.Now}] User Error: {string.Join(",", errors.Select(e => e.Message))}");
        Console.ForegroundColor = previousColor;
        return Task.CompletedTask;
    }
}