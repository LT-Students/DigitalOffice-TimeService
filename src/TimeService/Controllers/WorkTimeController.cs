using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        [HttpPost("edit")]
        public bool Edit(
            [FromBody] EditWorkTimeRequest request,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(request);
        }

        [HttpPost("add")]
        public Guid Add(
            [FromBody] CreateWorkTimeRequest workTime,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime);
        }
    }
}
