using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces
{
  [AutoInject]
  public interface IPatchDbWorkTimeDayJobMapper
  {
    JsonPatchDocument<DbWorkTimeDayJob> Map(JsonPatchDocument<EditWorkTimeDayJobRequest> request);
  }
}
