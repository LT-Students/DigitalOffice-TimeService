using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Position;
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
    private readonly IDepartmentInfoMapper _departmentInfoMapper;
    private readonly ICompanyUserInfoMapper _companyUserInfoMapper;

    public UserStatInfoMapper(
      IWorkTimeMonthLimitInfoMapper monthLimitInfoMapper,
      IWorkTimeInfoMapper workTimeInfoMapper,
      ILeaveTimeInfoMapper leaveTimeInfoMapper,
      IPositionInfoMapper positionInfoMapper,
      IDepartmentInfoMapper departmentInfoMapper,
      ICompanyUserInfoMapper companyUserInfoMapper)
    {
      _monthLimitInfoMapper = monthLimitInfoMapper;
      _workTimeInfoMapper = workTimeInfoMapper;
      _leaveTimeInfoMapper = leaveTimeInfoMapper;
      _positionInfoMapper = positionInfoMapper;
      _departmentInfoMapper = departmentInfoMapper;
      _companyUserInfoMapper = companyUserInfoMapper;
    }

    public UserStatInfo Map(
      UserInfo user,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<DbLeaveTime> leaveTimes,
      List<ProjectInfo> projects,
      PositionData position,
      DepartmentData department,
      CompanyUserData companyUser)
    {
      return new UserStatInfo
      {
        User = user,
        Position = _positionInfoMapper.Map(position),
        Department = _departmentInfoMapper.Map(department),
        CompanyUser = _companyUserInfoMapper.Map(companyUser),
        LeaveTimes = leaveTimes?.Select(lt => _leaveTimeInfoMapper.Map(lt)).ToList(),
        WorkTimes = workTimes?.Select(wt => _workTimeInfoMapper.Map(wt, projects?.FirstOrDefault(p => p.Id == wt.ProjectId))).ToList(),
        LimitInfo = _monthLimitInfoMapper.Map(monthLimit)
      };
    }
  }
}
