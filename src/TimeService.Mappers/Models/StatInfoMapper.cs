using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class StatInfoMapper : IStatInfoMapper
  {
    private readonly IWorkTimeMonthLimitInfoMapper _monthLimitInfoMapper;
    private readonly IWorkTimeDayJobInfoMapper _workTimeDayJobInfoMapper;
    private readonly IWorkTimeInfoMapper _workTimeInfoMapper;
    private readonly ILeaveTimeInfoMapper _leaveTimeInfoMapper;

    public StatInfoMapper(
      IWorkTimeMonthLimitInfoMapper monthLimitInfoMapper,
      IWorkTimeDayJobInfoMapper workTimeDayJobInfoMapper,
      IWorkTimeInfoMapper workTimeInfoMapper,
      ILeaveTimeInfoMapper leaveTimeInfoMapper)
    {
      _monthLimitInfoMapper = monthLimitInfoMapper;
      _workTimeDayJobInfoMapper = workTimeDayJobInfoMapper;
      _workTimeInfoMapper = workTimeInfoMapper;
      _leaveTimeInfoMapper = leaveTimeInfoMapper;
    }

    public StatInfo Map(
      Guid userId,
      UserInfo user,
      ProjectUserData projectUser,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<ProjectInfo> projects,
      List<DbLeaveTime> leaveTimes)
    {
      return new StatInfo
      {
        User = user ?? new UserInfo { Id = userId },
        LeaveTimes = leaveTimes?.Select(lt => _leaveTimeInfoMapper.Map(lt)).ToList(),
        WorkTimes = workTimes?.Select(wt => _workTimeInfoMapper.Map(wt, projectUser, projects.FirstOrDefault(p => p.Id == wt.ProjectId))).ToList(),
        LimitInfo = _monthLimitInfoMapper.Map(monthLimit)
      };
    }
  }
}
