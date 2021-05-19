using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
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
        [HttpPost("add")]
        public Guid Add(
            [FromBody] CreateWorkTimeRequest workTime,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime);
        }

        [HttpPatch("edit")]
        public bool Edit(
            [FromQuery] Guid workTimeId,
            [FromBody] JsonPatchDocument<EditWorkTimeRequest> request,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(workTimeId, request);
        }
    }
}
