using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class WorkTimeInfoMapper : IWorkTimeInfoMapper
    {
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
                Day = dbWorkTime.Day,
                UserHours = dbWorkTime.UserHours,
                ManagerHours = dbWorkTime.ManagerHours,
                Description = dbWorkTime.Description
            };
        }
    }
}
