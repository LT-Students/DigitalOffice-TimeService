using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class WorkTimeInfoMapper : IWorkTimeInfoMapper
    {
        public WorkTimeInfo Map(DbWorkTime dbWorkTime)
        {
            if (dbWorkTime == null)
            {
                throw new ArgumentNullException(nameof(dbWorkTime));
            }

            return new WorkTimeInfo
            {
                Id = dbWorkTime.Id,
                UserId = dbWorkTime.UserId,
                ProjectId = dbWorkTime.ProjectId,
                CreatedBy = dbWorkTime.CreatedBy,
                StartTime = dbWorkTime.StartTime,
                EndTime = dbWorkTime.EndTime,
                CreatedAt = dbWorkTime.CreatedAt,
                Title = dbWorkTime.Title,
                Description = dbWorkTime.Description
            };
        }
    }
}
