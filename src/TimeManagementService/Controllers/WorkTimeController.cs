using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        [HttpPatch("editWorkTime")]
        public bool EditWorkTime(
            [FromQuery] EditWorkTimeRequest request,
            [FromServices] IEditWorkTimeCommand command)
        {
            return command.Execute(request);
        }

        [HttpPost("addWorkTime")]
        public Guid AddWorkTime(
            [FromBody] WorkTime workTime,
            [FromServices] ICreateWorkTimeCommand command)
        {
            return command.Execute(workTime);
        }
    }
}
