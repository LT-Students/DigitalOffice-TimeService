using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        [HttpPost("editWorkTime")]
        public bool EditWorkTime(
            [FromBody] EditWorkTimeRequest request,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(request);
        }

        [HttpPost("addWorkTime")]
        public Guid AddWorkTime(
            [FromBody] CreateWorkTimeRequest workTime,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime);
        }
    }
}
