using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
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
        public LeaveTimesResponse Find(
            [FromServices] IFindLeaveTimesCommand command,
            [FromQuery] FindLeaveTimesFilter filter,
            [FromQuery] int skipPagesCount,
            [FromQuery] int takeCount)
        {
            return command.Execute(filter, skipPagesCount, takeCount);
        }
    }
}
