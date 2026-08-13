using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Models;

namespace ConsoleAppDataPipeline.Sinks;

public class UserConsoleRecordSink : IRecordSink<User>
{
    public Task WriteAsync(User record, CancellationToken cancellationToken = default)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[{DateTime.Now}] User(Name: {record.Name}, Age: {record.Age}, Email: {record.Email})");
        Console.ForegroundColor = previousColor;
        return Task.CompletedTask;
    }
}