using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
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
    public class LeaveTimeController : ControllerBase
    {
        [HttpPost("add")]
        public OperationResultResponse<Guid> Add(
            [FromBody] CreateLeaveTimeRequest leaveTime,
            [FromServices] ICreateLeaveTimeCommand command)
        {
            return command.Execute(leaveTime);
        }

        [HttpGet("find")]
        public FindResultResponse<LeaveTimeResponse> Find(
            [FromServices] IFindLeaveTimesCommand command,
            [FromQuery] FindLeaveTimesFilter filter)
        {
            return command.Execute(filter);
        }

        [HttpPatch("edit")]
        public OperationResultResponse<bool> Edit(
            [FromServices] IEditLeaveTimeCommand command,
            [FromQuery] Guid leaveTimeId,
            [FromBody] JsonPatchDocument<EditLeaveTimeRequest> request)
        {
            return command.Execute(leaveTimeId, request);
        }
    }
}
