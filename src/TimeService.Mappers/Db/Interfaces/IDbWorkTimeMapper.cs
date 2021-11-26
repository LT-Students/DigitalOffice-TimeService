using System;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbWorkTimeMapper
  {
    DbWorkTime Map(DbWorkTime parent, Guid managerId);
  }
}
