using ConsoleAppDataPipeline.Mappers;
using ConsoleAppDataPipeline.Models;

namespace ConsoleAppDataPipeline.Interfaces;

public interface IRecordMapper<T> where T : class
{
    MappingResult<T> Map(RawRecord record);
}