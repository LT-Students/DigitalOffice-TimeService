using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
    [AutoInject]
    public interface IDbWorkTimeMapper
    {
        DbWorkTime Map(CreateWorkTimeRequest leaveTime, Guid createdBy);
    }
}
