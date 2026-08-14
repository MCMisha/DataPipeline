using DataPipeline.Core.Models;

namespace DataPipeline.Core.Interfaces;

public interface IRecordMapper<T> where T : class
{
    MappingResult<T> Map(RawRecord record);
}