using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class WorkTimeDayJobController : Controller
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid>> CreateAsync(
      [FromServices] ICreateWorkTimeDayJobCommand command,
      [FromBody] CreateWorkTimeDayJobRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> EditAsync(
      [FromServices] IEditWorkTimeDayJobCommand command,
      [FromQuery] Guid workTimeDayJobId,
      [FromBody] JsonPatchDocument<EditWorkTimeDayJobRequest> request)
    {
      return await command.ExecuteAsync(workTimeDayJobId, request);
    }
  }
}
