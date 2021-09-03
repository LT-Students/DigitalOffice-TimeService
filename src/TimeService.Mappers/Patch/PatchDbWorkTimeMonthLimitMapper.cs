using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Patch
{
  public class PatchDbWorkTimeMonthLimitMapper : IPatchDbWorkTimeMonthLimitMapper
  {
    public JsonPatchDocument<DbWorkTimeMonthLimit> Map(JsonPatchDocument<EditWorkTimeMonthLimitRequest> request)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      JsonPatchDocument<DbWorkTimeMonthLimit> dbRequest = new();

      foreach (var item in request.Operations)
      {
        dbRequest.Operations.Add(new Operation<DbWorkTimeMonthLimit>(item.op, item.path, item.from, item.value));
      }

      return dbRequest;
    }
  }
}
