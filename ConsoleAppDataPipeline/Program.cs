using System.Text.Json;
using ConsoleAppDataPipeline.Builders;
using ConsoleAppDataPipeline.DataSources;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;
using ConsoleAppDataPipeline.Processors;
using ConsoleAppDataPipeline.Runners;
using ConsoleAppDataPipeline.Sinks;


// IDataSource<User> dataSource = new CsvDataSource<User>();
// UserMapper userMapper = new UserMapper();
// await using var recordSink = new JsonRecordSink<User>(
//     "output/users.json",
//     new JsonWriterOptions
//     {
//         Indented = true
//     });
// await using var errorSink = new JsonErrorSink(
//     "output/errors.json",
//     new JsonWriterOptions
//     {
//         Indented = true
//     });
// await using Stream fileStream = File.OpenRead("input/test.csv");
// IPipelineRunner<User> pipelineRunner = new PipelineRunner<User>(dataSource, userMapper, recordSink, errorSink);
// await pipelineRunner.RunAsync(fileStream);
// Console.WriteLine(pipelineRunner.GetStatistics());

// IProcessorPipeline<User> processorPipeline = new ProcessorPipeline<User>(
//     [new ConditionalProcessor<User>(new)]);
// var user = new User("Adam", "mail@outlook.com", 23, 'M');
// var updatedUser = processorPipeline.Process(user);
//
// Console.WriteLine(updatedUser);

var conditionalProcessor = new ConditionalProcessor<User>(
    user => user.Age >= 18,
    user => user with { Name = "Adult " + user.Name });
var addPrefixToName =
    new TransformationProcessor<User>(user =>
        user with { Name = user.Gender.Equals('F') ? $"Mrs. {user.Name}" : $"Mr. {user.Name}", });
IProcessorPipeline<User> pipeline = new PipelineBuilder<User>()
    .AddProcessor(addPrefixToName)
    .AddProcessor(conditionalProcessor)
    .Build();

User user = new User("Anna", "adam@mail.com", 18, 'M');

var modifiedUser = pipeline.Process(user);

Console.WriteLine(modifiedUser);