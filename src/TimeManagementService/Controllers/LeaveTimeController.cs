using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
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
            [FromBody] LeaveTime leaveTime,
            [FromServices] ICreateLeaveTimeCommand command)
        {
            return command.Execute(leaveTime);
        }
    }
}
