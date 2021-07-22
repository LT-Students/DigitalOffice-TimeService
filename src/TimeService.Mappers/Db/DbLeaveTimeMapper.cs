using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
    public class DbLeaveTimeMapper : IDbLeaveTimeMapper
    {
        public DbLeaveTime Map(CreateLeaveTimeRequest request, Guid createdBy)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CreatedBy = createdBy,
                LeaveType = (int)request.LeaveType,
                Minutes = request.Minutes,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.Now,
                Comment = request.Comment
            };
        }
    }
}
