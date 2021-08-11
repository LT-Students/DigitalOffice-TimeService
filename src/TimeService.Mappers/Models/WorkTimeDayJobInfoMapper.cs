using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class WorkTimeDayJobInfoMapper : IWorkTimeDayJobInfoMapper
    {
        public WorkTimeDayJobInfo Map(DbWorkTimeDayJob dbWorkTimeDayJob)
        {
            if (dbWorkTimeDayJob == null)
            {
                throw new ArgumentNullException(nameof(dbWorkTimeDayJob));
            }

            return new WorkTimeDayJobInfo
            {
                Id = dbWorkTimeDayJob.Id,
                WorkTimeId = dbWorkTimeDayJob.WorkTimeId,
                Day = dbWorkTimeDayJob.Day,
                Description = dbWorkTimeDayJob.Description,
                Minutes = dbWorkTimeDayJob.Minutes,
                Name = dbWorkTimeDayJob.Name,
                IsActive = dbWorkTimeDayJob.IsActive
            };
        }
    }
}
