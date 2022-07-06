using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class UserStatInfoMapper : IUserStatInfoMapper
  {
    private readonly IWorkTimeMonthLimitInfoMapper _monthLimitInfoMapper;
    private readonly IWorkTimeInfoMapper _workTimeInfoMapper;
    private readonly ILeaveTimeInfoMapper _leaveTimeInfoMapper;
    private readonly IPositionInfoMapper _positionInfoMapper;
    private readonly ICompanyUserInfoMapper _companyUserInfoMapper;

    public UserStatInfoMapper(
      IWorkTimeMonthLimitInfoMapper monthLimitInfoMapper,
      IWorkTimeInfoMapper workTimeInfoMapper,
      ILeaveTimeInfoMapper leaveTimeInfoMapper,
      IPositionInfoMapper positionInfoMapper,
      ICompanyUserInfoMapper companyUserInfoMapper)
    {
      _monthLimitInfoMapper = monthLimitInfoMapper;
      _workTimeInfoMapper = workTimeInfoMapper;
      _leaveTimeInfoMapper = leaveTimeInfoMapper;
      _positionInfoMapper = positionInfoMapper;
      _companyUserInfoMapper = companyUserInfoMapper;
    }

    public UserStatInfo Map(
      Guid userId,
      UserInfo user,
      ProjectUserData projectUser,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<ProjectInfo> projects,
      List<DbLeaveTime> leaveTimes,
      PositionData position,
      CompanyUserData companyUser)
    {
      return new UserStatInfo
      {
        User = user ?? new UserInfo { Id = userId },
        Position = _positionInfoMapper.Map(position),
        CompanyUserInfo = _companyUserInfoMapper.Map(companyUser),
        LeaveTimes = leaveTimes?.Select(lt => _leaveTimeInfoMapper.Map(lt)).ToList(),
        WorkTimes = workTimes?.Select(wt => _workTimeInfoMapper.Map(wt, projectUser, projects.FirstOrDefault(p => p.Id == wt.ProjectId))).ToList(),
        LimitInfo = _monthLimitInfoMapper.Map(monthLimit)
      };
    }
  }
}
