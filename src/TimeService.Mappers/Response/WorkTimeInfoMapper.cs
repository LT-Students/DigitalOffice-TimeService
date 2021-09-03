using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Response
{
  public class WorkTimeResponseMapper : IWorkTimeResponseMapper
  {
    private readonly IWorkTimeInfoMapper _workTimeInfoMapper;
    private readonly IWorkTimeMonthLimitInfoMapper _workTimeMonthLimitInfoMapper;

    public WorkTimeResponseMapper(
      IWorkTimeInfoMapper workTimeInfoMapper,
      IWorkTimeMonthLimitInfoMapper workTimeMonthLimitInfoMapper)
    {
      _workTimeInfoMapper = workTimeInfoMapper;
      _workTimeMonthLimitInfoMapper = workTimeMonthLimitInfoMapper;
    }

    public WorkTimeResponse Map(
        DbWorkTime dbWorkTime,
        DbWorkTimeMonthLimit dbMonthLimit,
        UserInfo userInfo,
        ProjectUserData projectUser,
        ProjectInfo project)
    {
      if (dbWorkTime == null)
      {
        throw new ArgumentNullException(nameof(dbWorkTime));
      }

      return new WorkTimeResponse
      {
        WorkTime = _workTimeInfoMapper.Map(dbWorkTime, projectUser, project),
        User = userInfo ?? new UserInfo { Id = dbWorkTime.UserId },
        LimitInfo = _workTimeMonthLimitInfoMapper.Map(dbMonthLimit)
      };
    }
  }
}
