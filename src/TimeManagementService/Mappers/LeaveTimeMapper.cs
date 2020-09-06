using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers
{
    public class LeaveTimeMapper : IMapper<CreateLeaveTimeRequest, DbLeaveTime>
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
                LeaveType = request.LeaveType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Comment = request.Comment
            };
        }
    }
}
