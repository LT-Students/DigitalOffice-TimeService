using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Response
{
  public class LeaveTimeResponseMapper : ILeaveTimeResponseMapper
  {
    private readonly ILeaveTimeInfoMapper _leaveTimeInfoMapper;

    public LeaveTimeResponseMapper(
      ILeaveTimeInfoMapper leaveTimeInfoMapper)
    {
      _leaveTimeInfoMapper = leaveTimeInfoMapper;
    }

    public LeaveTimeResponse Map(DbLeaveTime dbLeaveTime, UserInfo user, UserInfo manager)
    {
      if (dbLeaveTime is null)
      {
        return null;
      }

      return new LeaveTimeResponse
      {
        LeaveTime = _leaveTimeInfoMapper.Map(dbLeaveTime, manager),
        User = user ?? new UserInfo { Id = dbLeaveTime.UserId },
      };
    }
  }
}
