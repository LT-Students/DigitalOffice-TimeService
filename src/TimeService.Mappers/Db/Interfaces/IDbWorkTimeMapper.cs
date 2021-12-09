using System;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbWorkTimeMapper
  {
    DbWorkTime Map(DbWorkTime parent, Guid managerId);

    DbWorkTime Map(CreateWorkTimeRequest request, Guid userId);
  }
}
