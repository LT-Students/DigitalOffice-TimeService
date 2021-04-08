using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Requests
{
    public class DbLeaveTimeMapper : IDbLeaveTimeMapper
    {
        public DbLeaveTime Map(CreateLeaveTimeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                WorkerUserId = request.WorkerUserId,
                LeaveType = (int)request.LeaveType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Comment = request.Comment
            };
        }
    }
}
