namespace MappingEngineService.Models;

public class MappingStatistic
{
    public int FieldsMapped { get; set; }
    public int FieldsSkipped { get; set; }
    public int Errors { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
