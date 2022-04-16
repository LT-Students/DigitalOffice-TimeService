using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IStatInfoMapper
  {
    StatInfo Map(
      Guid userId,
      UserInfo user,
      ProjectUserData projectUser,
      DbWorkTimeMonthLimit monthLimit,
      List<DbWorkTime> workTimes,
      List<ProjectInfo> projects,
      List<DbLeaveTime> leaveTimes);
  }
}
