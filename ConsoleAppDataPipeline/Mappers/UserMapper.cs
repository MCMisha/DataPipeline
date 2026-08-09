using System.Text.RegularExpressions;
using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Models;

namespace ConsoleAppDataPipeline.Mappers;

public class UserMapper : IRecordMapper<User>
{
    private const string EmailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
    public MappingResult<User> Map(RawRecord record)
    {
        var errors = new List<MappingError>();

        var name = record.GetValueOrDefault("Name");

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(new MappingError(
                "Name",
                "Name is required",
                name));
        }
        else if (name.Length > 100)
        {
            errors.Add(new MappingError(
                "Name",
                "Name must be less than 100 characters",
                name));
        }

        var email = record.GetValueOrDefault("Email");
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(new MappingError(
                "Email",
                "Email is required",
                email));
        } 
        else if (email.Length > 254)
        {
            errors.Add(new MappingError(
                "Email",
                "Email must be less than 254 characters",
                email));
        } 
        else if (!Regex.IsMatch(email, EmailPattern))
        {
            errors.Add(new MappingError(
                "Email",
                "Email is invalid",
                email));
        }

        var ageText = record.GetValueOrDefault("Age");

        if (!int.TryParse(ageText, out int age))
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

        if (errors.Count > 0)
        {
            return MappingResult<User>.Failure(record.RowNumber, errors.ToArray());
        }

        return MappingResult<User>.Success(
            new User(name!, email, age));
    }
}