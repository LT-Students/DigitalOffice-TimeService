using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Filters;
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
        [HttpPost("add")]
        public OperationResultResponse<Guid> Add(
            [FromBody] CreateWorkTimeRequest workTime,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime);
        }

        [HttpGet("find")]
        public WorkTimesResponse Find(
            [FromServices] IFindWorkTimesCommand command,
            [FromQuery] FindWorkTimesFilter filter,
            [FromQuery] int skipPagesCount,
            [FromQuery] int takeCount)
        {
            return command.Execute(filter, skipPagesCount, takeCount);
        }

        [HttpPatch("edit")]
        public OperationResultResponse<bool> Edit(
            [FromQuery] Guid workTimeId,
            [FromBody] JsonPatchDocument<EditWorkTimeRequest> request,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(workTimeId, request);
        }
    }
}
