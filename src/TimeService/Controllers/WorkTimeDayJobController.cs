using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace LT.DigitalOffice.TimeService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkTimeDayJobController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkTimeDayJobController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("create")]
        public OperationResultResponse<Guid> Create(
            [FromServices] ICreateWorkTimeDayJobCommand command,
            [FromBody] CreateWorkTimeDayJobRequest request)
        {
            OperationResultResponse<Guid> result =  command.Execute(request);

            if (result.Status ==  OperationResultStatusType.FullSuccess)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
            }

            return result;
        }

        [HttpPatch("edit")]
        public OperationResultResponse<bool> Edit(
            [FromServices] IEditWorkTimeDayJobCommand command,
            [FromQuery] Guid workTimeDayJobId,
            [FromBody] JsonPatchDocument<EditWorkTimeDayJobRequest> request)
        {
            return command.Execute(workTimeDayJobId, request);
        }
    }
}
