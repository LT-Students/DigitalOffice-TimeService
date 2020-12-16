using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        [HttpPost("getUserWorkTimes")]
        public IEnumerable<WorkTimeResponse> GetUserWorkTimes(
            [FromQuery] Guid userId,
            [FromBody] WorkTimeFilter filter,
            [FromServices] IGetUserWorkTimesCommand command)
        {
            return command.Execute(userId, filter);
        }

        [HttpPost("addWorkTime")]
        public Guid AddWorkTime(
            [FromBody] WorkTimeRequest workTime,
            [FromHeader] Guid currentUserId,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime, currentUserId);
        }

        [HttpPatch("editWorkTime")]
        public bool EditWorkTime(
            [FromQuery] EditWorkTimeRequest request,
            [FromHeader] Guid currentUserId,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(request, currentUserId);
        }
    }
}
