using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class LeaveTimeController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid?>> AddAsync(
      [FromBody] CreateLeaveTimeRequest leaveTime,
      [FromServices] ICreateLeaveTimeCommand command)
    {
      return await command.ExecuteAsync(leaveTime);
    }

    [HttpGet("find")]
    public async Task<FindResultResponse<LeaveTimeResponse>> FindAsync(
      [FromServices] IFindLeaveTimesCommand command,
      [FromQuery] FindLeaveTimesFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> EditAsync(
      [FromServices] IEditLeaveTimeCommand command,
      [FromQuery] Guid leaveTimeId,
      [FromBody] JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      return await command.ExecuteAsync(leaveTimeId, request);
    }
  }
}
