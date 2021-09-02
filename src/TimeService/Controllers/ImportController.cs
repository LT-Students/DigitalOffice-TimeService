﻿using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.Import.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class ImportController : ControllerBase
  {
    [HttpGet("get")]
    public OperationResultResponse<byte[]> Get(
      [FromServices] IImportStatCommand command,
      [FromQuery] ImportStatFilter filter)
    {
      return command.Execute(filter);
    }
  }
}
