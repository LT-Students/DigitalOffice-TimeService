using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers
{
    public class WorkTimeMapper : IWorkTimeMapper
    {
        public DbWorkTime Map(WorkTimeRequest value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new DbWorkTime
            {
                Id = Guid.NewGuid(),
                UserId = value.UserId ?? value.CurrentUserId,
                StartDate = value.StartDate,
                EndDate = value.EndDate,
                Title = value.Title,
                ProjectId = value.ProjectId,
                Description = value.Description,
                Minutes = value.Minutes,
                CreatedAt = DateTime.Now,
                CreatedBy = value.CurrentUserId
            };
        }

        public WorkTimeResponse Map(DbWorkTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new WorkTimeResponse()
            {
                Id = value.Id,
                UserId = value.UserId,
                StartDate = value.StartDate,
                EndDate = value.EndDate,
                Minutes = value.Minutes,
                Title = value.Title,
                ProjectId = value.ProjectId,
                Description = value.Description,
                CreatedAt = value.CreatedAt,
                CreatedBy = value.CreatedBy
            };
        }
    }
}
