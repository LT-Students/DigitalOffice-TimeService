using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IUserStatInfoMapper
  {
    UserStatInfo Map(
      UserInfo user,
      List<UserInfo> managersInfos,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<DbLeaveTime> leaveTimes,
      List<ProjectInfo> projects,
      PositionData position,
      DepartmentData department,
      CompanyUserData companyUser);
  }
}
