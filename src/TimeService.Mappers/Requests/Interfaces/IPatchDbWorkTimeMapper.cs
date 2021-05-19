using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces
{
    [AutoInject]
    public interface IPatchDbWorkTimeMapper
    {
        JsonPatchDocument<DbWorkTime> Map(JsonPatchDocument<EditWorkTimeRequest> request);
    }
}
