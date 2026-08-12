using System.Text.Json;
using ConsoleAppDataPipeline.Builders;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;
using ConsoleAppDataPipeline.Processors;
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

IProcessorPipeline<User> pipeline = new PipelineBuilder<User>()
    .Transform(user => user with { Name = user.Gender.Equals('F') ? $"Mrs. {user.Name}" : $"Mr. {user.Name}" })
    .If(user => user.Age >= 18)
    .Then(user => user with{ Name = "Adult " + user.Name})
    .Build();

IPipelineRunner<User> pipelineRunner = new PipelineRunner<User>(dataSource, userMapper, recordSink, errorSink, pipeline);
await pipelineRunner.RunAsync(fileStream);
Console.WriteLine(pipelineRunner.GetStatistics());
