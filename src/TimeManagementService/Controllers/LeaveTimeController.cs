using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTimeController : ControllerBase
    {
        [HttpPost("getUserLeaveTimes")]
        public IEnumerable<LeaveTime> GetUserWorkTimes(
            [FromQuery] Guid userId,
            [FromServices] IGetUserLeaveTimesCommand command)
        {
            return command.Execute(userId);
        }

        [HttpPost("addLeaveTime")]
        public Guid AddLeaveTime(
            [FromBody] LeaveTime leaveTime,
            [FromHeader] Guid currentUserId,
            [FromServices] ICreateLeaveTimeCommand command)
        {
            return command.Execute(leaveTime, currentUserId);
        }
    }
}
