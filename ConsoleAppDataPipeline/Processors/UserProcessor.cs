using ConsoleAppDataPipeline.Interfaces;
using ConsoleAppDataPipeline.Models;

namespace ConsoleAppDataPipeline.Processors;

public class UserProcessor : IProcessor<User>
{
    
    public User Process(User input)
    {
        string name = input.Name;
        if (input.Gender == 'F')
        {
            name = "Mrs. " + name;    
        }
        else
        {
            name = "Mr. " + name;
        }
        
        return input with { Name = name, Age = input.Age + 1 };
    }
}