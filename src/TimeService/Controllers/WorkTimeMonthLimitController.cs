using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class WorkTimeMonthLimitController : ControllerBase
  {
    [HttpGet("find")]
    public FindResultResponse<WorkTimeMonthLimitInfo> Find(
      [FromServices] IFindWorkTimeMonthLimitCommand command,
      [FromQuery] FindWorkTimeMonthLimitsFilter filter,
      [FromQuery] int skipCount,
      [FromQuery] int takeCount)
    {
      return command.Execute(filter, skipCount, takeCount);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> Edit(
      [FromServices] IEditWorkTimeMonthLimitCommand command,
      [FromQuery] Guid workTimeMonthLimitId,
      [FromBody] JsonPatchDocument<EditWorkTimeMonthLimitRequest> request)
    {
      return await command.ExecuteAsync(workTimeMonthLimitId, request);
    }
  }
}
