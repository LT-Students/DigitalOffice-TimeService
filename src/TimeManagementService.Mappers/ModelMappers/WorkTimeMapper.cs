using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers
{
    public class WorkTimeMapper : IWorkTimeMapper
    {
        public DbWorkTime Map(WorkTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new DbWorkTime
            {
                Id = Guid.NewGuid(),
                WorkerUserId = value.WorkerUserId,
                StartTime = value.StartTime,
                EndTime = value.EndTime,
                Title = value.Title,
                ProjectId = value.ProjectId,
                Description = value.Description
            };
        }

        public WorkTime Map(DbWorkTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new WorkTime()
            {
                Id = value.Id,
                WorkerUserId = value.WorkerUserId,
                StartTime = value.StartTime,
                EndTime = value.EndTime,
                Title = value.Title,
                ProjectId = value.ProjectId,
                Description = value.Description
            };
        }
    }
}
