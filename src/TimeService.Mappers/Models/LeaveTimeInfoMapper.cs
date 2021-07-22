using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class LeaveTimeInfoMapper : ILeaveTimeInfoMapper
    {
        public LeaveTimeInfo Map(DbLeaveTime dbLeaveTime)
        {
            if (dbLeaveTime == null)
            {
                throw new ArgumentNullException(nameof(dbLeaveTime));
            }

            return new LeaveTimeInfo
            {
                Id = dbLeaveTime.Id,
                UserId = dbLeaveTime.UserId,
                CreatedBy = dbLeaveTime.CreatedBy,
                Minutes = dbLeaveTime.Minutes,
                StartTime = dbLeaveTime.StartTime,
                EndTime = dbLeaveTime.EndTime,
                CreatedAt = dbLeaveTime.CreatedAt,
                Comment = dbLeaveTime.Comment,
                LeaveType = (LeaveType)dbLeaveTime.LeaveType
            };
        }
    }
}
