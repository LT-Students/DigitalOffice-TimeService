using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTimeController : ControllerBase
    {
        [HttpPost("addLeaveTime")]
        public Guid AddLeaveTime(
            [FromBody] CreateLeaveTimeRequest leaveTime,
            [FromServices] ICreateLeaveTimeCommand command)
        {
            return command.Execute(leaveTime);
        }
    }
}
