using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeManagementController : ControllerBase
    {
        [HttpGet("editWorkTime")]
        public bool EditWorkTime(
            [FromBody] EditWorkTimeRequest request, 
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(request);
        }

        [HttpPost("addLeaveTime")]
        public Guid AddLeaveTime(
            [FromBody] CreateLeaveTimeRequest leaveTime,
            [FromServices] ICreateLeaveTimeCommand command)
        {
            return command.Execute(leaveTime);
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
