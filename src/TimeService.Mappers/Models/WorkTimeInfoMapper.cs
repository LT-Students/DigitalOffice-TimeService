using System.Linq;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class WorkTimeInfoMapper : IWorkTimeInfoMapper
  {
    private readonly IWorkTimeDayJobInfoMapper _workTimeDayJobInfoMapper;

    public WorkTimeInfoMapper(
      IWorkTimeDayJobInfoMapper workTimeDayJobInfoMapper)
    {
      _workTimeDayJobInfoMapper = workTimeDayJobInfoMapper;
    }

    public WorkTimeInfo Map(
      DbWorkTime dbWorkTime,
      ProjectInfo project)
    {
      if (dbWorkTime == null)
      {
        return null;
      }

      return new WorkTimeInfo
      {
        Id = dbWorkTime.Id,
        Project = project
          ?? new ProjectInfo
          {
            Id = dbWorkTime.ProjectId
          },
        Year = dbWorkTime.Year,
        Month = dbWorkTime.Month,
        UserHours = dbWorkTime.Hours,
        ManagerHours = dbWorkTime.ManagerWorkTime?.Hours,
        Description = dbWorkTime.Description,
        ManagerDescription = dbWorkTime.ManagerWorkTime?.Description,
        ModifiedAtUtc = dbWorkTime.ModifiedAtUtc,
        Jobs = dbWorkTime.WorkTimeDayJobs?.Select(_workTimeDayJobInfoMapper.Map).ToList()
      };
    }
  }
}
