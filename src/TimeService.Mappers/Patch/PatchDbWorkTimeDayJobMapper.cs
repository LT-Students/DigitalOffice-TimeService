using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Patch
{
  public class PatchDbWorkTimeDayJobMapper : IPatchDbWorkTimeDayJobMapper
  {
    public JsonPatchDocument<DbWorkTimeDayJob> Map(JsonPatchDocument<EditWorkTimeDayJobRequest> request)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      JsonPatchDocument<DbWorkTimeDayJob> dbRequest = new();

      foreach (var item in request.Operations)
      {
        if (item.path.EndsWith(nameof(EditWorkTimeDayJobRequest.Name), StringComparison.OrdinalIgnoreCase)
            || item.path.EndsWith(nameof(EditWorkTimeDayJobRequest.Description), StringComparison.OrdinalIgnoreCase))
        {
          dbRequest.Operations.Add(new Operation<DbWorkTimeDayJob>(item.op, item.path, item.from, item.value?.ToString().Trim()));
          continue;
        }

        dbRequest.Operations.Add(new Operation<DbWorkTimeDayJob>(item.op, item.path, item.from, item.value));
      }

      return dbRequest;
    }
  }
}
