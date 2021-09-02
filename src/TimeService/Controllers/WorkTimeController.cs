using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
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
        public FindResultResponse<WorkTimeResponse> Find(
            [FromServices] IFindWorkTimesCommand command,
            [FromQuery] FindWorkTimesFilter filter)
        {
            return command.Execute(filter);
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
