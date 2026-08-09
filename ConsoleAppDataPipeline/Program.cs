using System.Text.Json;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;
using ConsoleAppDataPipeline.Runners;
using ConsoleAppDataPipeline.Sinks;


IDataSource<User> dataSource = new CsvDataSource<User>();
UserMapper userMapper = new UserMapper();
await using var recordSink = new JsonRecordSink<User>(
    "output/users.json",
    new JsonWriterOptions
    {
        Indented = true
    });
await using var errorSink = new JsonErrorSink(
    "output/errors.json",
    new JsonWriterOptions
    {
        Indented = true
    });
await using Stream fileStream = File.OpenRead("input/test.csv");
IPipelineRunner<User> pipelineRunner = new PipelineRunner<User>(dataSource, userMapper, recordSink, errorSink);
await pipelineRunner.RunAsync(fileStream);
Console.WriteLine(pipelineRunner.GetStatistics());

