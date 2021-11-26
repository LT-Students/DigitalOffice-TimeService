using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class WorkTimeController : ControllerBase
  {
    [HttpGet("find")]
    public async Task<FindResultResponse<WorkTimeResponse>> FindAsync(
      [FromServices] IFindWorkTimesCommand command,
      [FromQuery] FindWorkTimesFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> Edit(
      [FromServices] IEditWorkTimeCommand command,
      [FromQuery] Guid workTimeId,
      [FromBody] JsonPatchDocument<EditWorkTimeRequest> request)
    {
      return await command.ExecuteAsync(workTimeId, request);
    }
  }
}
