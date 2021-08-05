using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
    public class DbLeaveTimeMapper : IDbLeaveTimeMapper
    {
        public DbLeaveTime Map(CreateLeaveTimeRequest request, Guid createdBy, List<Guid> userIds)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                UserId = userIds.FirstOrDefault(),
                CreatedBy = createdBy,
                LeaveType = (int)request.LeaveType,
                Minutes = request.Minutes,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.Now,
                Comment = !string.IsNullOrEmpty(request.Comment?.Trim()) ? request.Comment.Trim() : null,
                IsActive = true
            };
        }
    }
}
