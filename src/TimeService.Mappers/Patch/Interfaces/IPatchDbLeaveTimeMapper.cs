using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces
{
  [AutoInject]
  public interface IPatchDbLeaveTimeMapper
  {
    JsonPatchDocument<DbLeaveTime> Map(JsonPatchDocument<EditLeaveTimeRequest> request);
  }
}
