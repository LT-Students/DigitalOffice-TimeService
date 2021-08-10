using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
    [AutoInject]
    public interface IPatchDbWorkTimeMapper
    {
        JsonPatchDocument<DbWorkTime> Map(JsonPatchDocument<EditWorkTimeRequest> request);
    }
}
