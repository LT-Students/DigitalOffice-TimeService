using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
    public class DbWorkTimeMapper : IDbWorkTimeMapper
    {
        public DbWorkTime Map(EditWorkTimeRequest request, DbWorkTime oldDbWorkTime)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbWorkTime()
            {
                Id = request.Id,
                UserId = request.UserId,
                ProjectId = request.ProjectId,
                CreatedBy = oldDbWorkTime.CreatedBy,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = oldDbWorkTime.CreatedAt,
                Title = request.Title,
                Description = request.Description
            };
        }

        public DbWorkTime Map(CreateWorkTimeRequest request, Guid createdBy)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbWorkTime
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ProjectId = request.ProjectId,
                CreatedBy = createdBy,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.Now,
                Title = request.Title,
                Description = request.Description
            };
        }
    }
}
