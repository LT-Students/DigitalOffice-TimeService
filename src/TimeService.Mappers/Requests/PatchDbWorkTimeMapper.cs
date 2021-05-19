using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Requests
{
    public class PatchDbWorkTimeMapper : IPatchDbWorkTimeMapper
    {
        public JsonPatchDocument<DbWorkTime> Map(JsonPatchDocument<EditWorkTimeRequest> request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            JsonPatchDocument<DbWorkTime> dbRequest = new();

            foreach (var item in request.Operations)
            {
                dbRequest.Operations.Add(new Operation<DbWorkTime>(item.op, item.path, item.from, item.value));
            }

            return dbRequest;
        }
    }
}
