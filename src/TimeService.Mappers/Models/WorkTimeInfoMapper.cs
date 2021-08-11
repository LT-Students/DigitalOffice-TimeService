using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class WorkTimeInfoMapper : IWorkTimeInfoMapper
    {
        private readonly IWorkTimeDayJobInfoMapper _workTimeDayJobInfoMapper;

        public WorkTimeInfoMapper(IWorkTimeDayJobInfoMapper workTimeDayJobInfoMapper)
        {
            _workTimeDayJobInfoMapper = workTimeDayJobInfoMapper;
        }

        public WorkTimeInfo Map(DbWorkTime dbWorkTime, ProjectInfo project)
        {
            if (dbWorkTime == null)
            {
                throw new ArgumentNullException(nameof(dbWorkTime));
            }

            return new WorkTimeInfo
            {
                Id = dbWorkTime.Id,
                UserId = dbWorkTime.UserId,
                Project = project,
                Year = dbWorkTime.Year,
                Month = dbWorkTime.Month,
                UserHours = dbWorkTime.UserHours,
                ManagerHours = dbWorkTime.ManagerHours,
                Description = dbWorkTime.Description,
                Jobs = dbWorkTime.WorkTimeDayJobs?.Select(_workTimeDayJobInfoMapper.Map).ToList()
            };
        }
    }
}
