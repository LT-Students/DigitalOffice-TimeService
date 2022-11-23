using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Patch
{
  public class PatchDbLeaveTimeMapper : IPatchDbLeaveTimeMapper
  {
    public JsonPatchDocument<DbLeaveTime> Map(JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      JsonPatchDocument<DbLeaveTime> dbRequest = new();

      foreach (var item in request.Operations)
      {
        if (item.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase) ||
          item.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase))
        {
          dbRequest.Operations.Add(new Operation<DbLeaveTime>(
            item.op, item.path, item.from, DateTimeOffset.Parse(item.value.ToString()).UtcDateTime));
          continue;
        }

        dbRequest.Operations.Add(new Operation<DbLeaveTime>(item.op, item.path, item.from, item.value));
      }

      return dbRequest;
    }
  }
}
