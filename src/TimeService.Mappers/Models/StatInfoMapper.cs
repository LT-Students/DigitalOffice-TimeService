using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class StatInfoMapper : IStatInfoMapper
  {
    private readonly IDepartmentInfoMapper _departmentInfoMapper;
    private readonly IUserStatInfoMapper _userStatInfoMapper;

    public StatInfoMapper(
      IDepartmentInfoMapper departmentInfoMapper,
      IUserStatInfoMapper userStatInfoMapper)
    {
      _departmentInfoMapper = departmentInfoMapper;
      _userStatInfoMapper = userStatInfoMapper;
    }

    public List<StatInfo> Map(
      List<DepartmentData> departmentsInfos,
      List<Guid> usersIds,
      List<UserInfo> usersInfos,
      List<ProjectUserData> projectUsers,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<ProjectInfo> projects,
      List<DbLeaveTime> leaveTimes,
      List<PositionData> positions,
      List<CompanyUserData> companyUsers)
    {
      if (departmentsInfos is null || !departmentsInfos.Any())
      {
        return new List<StatInfo>
        {
          new StatInfo
          {
            DepartmentInfo = default,

            UsersStats = usersIds.Select(userId => _userStatInfoMapper.Map(
              userId,
              usersInfos?.FirstOrDefault(x => x.Id == userId),
              projectUsers?.FirstOrDefault(x => x.UserId == userId),
              monthLimit,
              workTimes?.Where(wt => wt.UserId == userId).ToList(),
              projects,
              leaveTimes.Where(lt => lt.UserId == userId).ToList(),
              positions?.FirstOrDefault(p => p.UsersIds.Contains(userId)),
              companyUsers?.FirstOrDefault(cu => cu.UserId == userId)))
            .ToList()
          }
        };
      }

      return departmentsInfos.Select(departmentInfo => new StatInfo
      {
        DepartmentInfo = _departmentInfoMapper.Map(departmentInfo),
        UsersStats = departmentInfo.Users.Select(user => _userStatInfoMapper.Map(
          user.UserId,
          usersInfos?.FirstOrDefault(x => x.Id == user.UserId),
          projectUsers?.FirstOrDefault(x => x.UserId == user.UserId),
          monthLimit,
          workTimes?.Where(wt => wt.UserId == user.UserId).ToList(),
          projects,
          leaveTimes.Where(lt => lt.UserId == user.UserId).ToList(),
          positions.FirstOrDefault(p => p.UsersIds.Contains(user.UserId)),
          companyUsers.FirstOrDefault(cu => cu.UserId == user.UserId)))
        .ToList()
      }).ToList();
    }
  }
}
