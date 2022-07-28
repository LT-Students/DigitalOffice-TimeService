using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class StatController : ControllerBase
  {
    [HttpGet("find")]
    public async Task<FindResultResponse<UserStatInfo>> FindAsync(
      [FromServices] IFindStatCommand command,
      [FromQuery] FindStatFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }
  }
}
