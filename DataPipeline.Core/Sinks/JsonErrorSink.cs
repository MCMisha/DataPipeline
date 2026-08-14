using System.Text.Json;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Models;

namespace DataPipeline.Demo.Sinks;

public class JsonErrorSink : IErrorSink, IAsyncDisposable
{
    private readonly FileStream _stream;
    private readonly Utf8JsonWriter _writer;
    private bool _disposed;
    
    public JsonErrorSink(
        string filePath,
        JsonWriterOptions writerOptions = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        _writer = new Utf8JsonWriter(_stream, writerOptions);
        _writer.WriteStartArray();
    }
    
    public async Task WriteAsync(
        IReadOnlyList<MappingError> errors,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentNullException.ThrowIfNull(errors);

        JsonSerializer.Serialize(_writer, errors);

        await _writer.FlushAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _writer.WriteEndArray();
        await _writer.FlushAsync();
        
        await _stream.DisposeAsync();
    }
}