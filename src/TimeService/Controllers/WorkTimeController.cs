using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        [HttpGet("find")]
        public FindResultResponse<WorkTimeInfo> Find(
            [FromServices] IFindWorkTimesCommand command,
            [FromQuery] FindWorkTimesFilter filter,
            [FromQuery] int skipCount,
            [FromQuery] int takeCount)
        {
            return command.Execute(filter, skipCount, takeCount);
        }

        [HttpPatch("edit")]
        public OperationResultResponse<bool> Edit(
            [FromServices] IEditWorkTimeCommand command,
            [FromQuery] Guid workTimeId,
            [FromBody] JsonPatchDocument<EditWorkTimeRequest> request)
        {
            return command.Execute(workTimeId, request);
        }
    }
}
