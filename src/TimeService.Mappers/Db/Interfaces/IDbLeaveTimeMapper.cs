using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
    [AutoInject]
    public interface IDbLeaveTimeMapper
    {
        DbLeaveTime Map(CreateLeaveTimeRequest leaveTime, Guid createdBy);
    }
}
