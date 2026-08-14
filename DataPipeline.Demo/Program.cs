using System.Text.Json;
using DataPipeline.Core.Builders;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Runners;
using DataPipeline.Core.Sinks;
using DataPipeline.Csv;
using DataPipeline.Demo.Mappers;
using DataPipeline.Demo.Models;
using DataPipeline.Demo.Models.Enums;
using DataPipeline.Demo.Sinks;

string Capitalize(string input)
{
    return char.ToUpper(input[0]) + input[1..].ToLower();
}


IDataSource<Employee> dataSource = new CsvDataSource<Employee>();
IRecordMapper<Employee> employeeMapper = new EmployeeMapper();
await using var recordSink = new JsonRecordSink<Employee>(
    "output/employees.json",
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
await using Stream fileStream = File.OpenRead("input/employees.csv");

IProcessorPipeline<Employee> pipeline = new PipelineBuilder<Employee>()
    .Transform(employee => employee with
    {
        FirstName = Capitalize(employee.FirstName),
        LastName = Capitalize(employee.LastName)
    })
    .If(employee => employee.Age >= 65)
    .Then(employee => employee with { Status = EmployeeStatus.Inactive})
    .If(employee => employee.Status == EmployeeStatus.Active && employee.Salary < 2000)
    .Then(employee => employee with {Salary = employee.Salary + 2000})
    .Build();

IPipelineRunner<Employee> pipelineRunner =
    new PipelineRunner<Employee>(dataSource, employeeMapper, recordSink, errorSink, pipeline);
await pipelineRunner.RunAsync(fileStream);
Console.WriteLine(pipelineRunner.GetStatistics());