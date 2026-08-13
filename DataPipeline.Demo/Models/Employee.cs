using DataPipeline.Demo.Models.Enums;

namespace DataPipeline.Demo.Models;

public record Employee(string FirstName, string LastName, 
    string Email, int Age, 
    double Salary, EmployeeStatus Status);