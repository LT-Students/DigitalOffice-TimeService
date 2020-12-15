using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers
{
    public class LeaveTimeMapper : ILeaveTimeMapper
    {
        public DbLeaveTime Map(LeaveTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                UserId = value.UserId ?? value.CurrentUserId,
                LeaveType = (int)value.LeaveType,
                StartTime = value.StartTime,
                EndTime = value.EndTime,
                Comment = value.Comment
            };
        }

        public LeaveTime Map(DbLeaveTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new LeaveTime
            {
                Id = value.Id,
                UserId = value.UserId,
                LeaveType = (LeaveType)value.LeaveType,
                StartTime = value.StartTime,
                EndTime = value.EndTime,
                Comment = value.Comment
            };
        }
    }
}
