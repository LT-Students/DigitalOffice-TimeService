using LT.DigitalOffice.Models.Broker.Models;
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

        public WorkTimeInfo Map(
            DbWorkTime dbWorkTime,
            UserInfo userInfo,
            ProjectUserData projectUser,
            ProjectInfo project,
            WorkTimeMonthLimitInfo monthLimitInfo)
        {
            if (dbWorkTime == null)
            {
                throw new ArgumentNullException(nameof(dbWorkTime));
            }

            return new WorkTimeInfo
            {
                Id = dbWorkTime.Id,
                User = userInfo ?? new UserInfo { Id = dbWorkTime.UserId },
                Project = project
                    ?? new ProjectInfo
                    {
                        Id = dbWorkTime.Id
                    },
                Year = dbWorkTime.Year,
                Month = dbWorkTime.Month,
                Day = projectUser?.CreatedAtUtc.Day,
                UserHours = dbWorkTime.UserHours,
                ManagerHours = dbWorkTime.ManagerHours,
                Description = dbWorkTime.Description,
                LimitInfo = monthLimitInfo,
                ModifiedAtUtc = dbWorkTime.ModifiedAtUtc,
                Jobs = dbWorkTime.WorkTimeDayJobs?.Select(_workTimeDayJobInfoMapper.Map).ToList()
            };
        }
    }
}
