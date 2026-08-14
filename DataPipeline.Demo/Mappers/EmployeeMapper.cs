using System.Text.RegularExpressions;
using DataPipeline.Core.Interfaces;
using DataPipeline.Core.Models;
using DataPipeline.Demo.Models;
using DataPipeline.Demo.Models.Enums;

namespace DataPipeline.Demo.Mappers;

public class EmployeeMapper : IRecordMapper<Employee>
{
    private const string EmailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";

    public MappingResult<Employee> Map(RawRecord record)
    {
        var errors = new List<MappingError>();

        var firstNameRaw = record.GetValueOrDefault("FirstName");
        string firstName = string.Empty;
        
        
        if (string.IsNullOrWhiteSpace(firstNameRaw))
        {
            errors.Add(new MappingError(
                "FirstName",
                "FirstName is required",
                firstNameRaw));
        }
        else if (firstNameRaw.Length > 100)
        {
            errors.Add(new MappingError(
                "FirstName",
                "FirstName must be less than 100 characters",
                firstNameRaw));
        }
        else
        {
            firstName = firstNameRaw;
        }

        var lastNameRaw = record.GetValueOrDefault("LastName");
        string lastName = string.Empty;
        if (string.IsNullOrWhiteSpace(lastNameRaw))
        {
            errors.Add(new MappingError(
                "LastName",
                "LastName is required",
                lastNameRaw));
        }
        else if (lastNameRaw.Length > 100)
        {
            errors.Add(new MappingError(
                "LastName",
                "LastName must be less than 100 characters",
                lastNameRaw));
        }
        else
        {
            lastName = lastNameRaw;
        }

        var emailRaw = record.GetValueOrDefault("Email");
        string email = string.Empty;
        if (string.IsNullOrWhiteSpace(emailRaw))
        {
            errors.Add(new MappingError(
                "Email",
                "Email is required",
                emailRaw));
        }
        else if (emailRaw.Length > 254)
        {
            errors.Add(new MappingError(
                "Email",
                "Email must be less than 254 characters",
                emailRaw));
        }
        else if (!Regex.IsMatch(emailRaw, EmailPattern))
        {
            errors.Add(new MappingError(
                "Email",
                "Email is invalid",
                emailRaw));
        }
        else
        {
            email = emailRaw;
        }

        int age = 0;
        var ageText = record.GetValueOrDefault("Age");
        if (string.IsNullOrWhiteSpace(ageText))
        {
            errors.Add(new MappingError(
                "Age",
                "Age is required",
                ageText));
        }
        else if (!int.TryParse(ageText, out age))
        {
            errors.Add(new MappingError(
                "Age",
                "Age must be an integer",
                ageText));
        }
        else if (age < 0)
        {
            errors.Add(new MappingError(
                "Age",
                "Age must be greater than or equal to zero",
                ageText));
        }

        double salary = default;
        var salaryText = record.GetValueOrDefault("Salary");
        if (string.IsNullOrEmpty(salaryText))
        {
            errors.Add(new MappingError(
                "Salary",
                "Salary is required",
                salaryText));
        }
        else if (!double.TryParse(salaryText, out salary))
        {
            errors.Add(new MappingError(
                "Salary",
                "Salary must be a double",
                salaryText));
        }
        else if (salary < 0)
        {
            errors.Add(new MappingError(
                "Salary",
                "Salary cannot be less than zero",
                salaryText));
        }

        var statusText = record.GetValueOrDefault("Status");
        EmployeeStatus status = EmployeeStatus.Active;
        if (string.IsNullOrEmpty(statusText))
        {
            errors.Add(new MappingError(
                "Status",
                "Status is required",
                statusText));
        }
        else if (!Enum.TryParse(statusText, out status))
        {
            errors.Add(new MappingError(
                "Status",
                "Status is invalid",
                statusText));
        }

        if (errors.Any())
        {
            return MappingResult<Employee>.Failure(record.RowNumber, errors.ToArray());
        }

        return MappingResult<Employee>.Success(new Employee(firstName, lastName, email, age, salary, status));
    }
}