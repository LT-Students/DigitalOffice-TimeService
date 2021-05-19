using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
    public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
    {
        private readonly ICreateLeaveTimeRequestValidator _validator;
        private readonly IDbLeaveTimeMapper _mapper;
        private readonly ILeaveTimeRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateLeaveTimeCommand(
            ICreateLeaveTimeRequestValidator validator,
            IDbLeaveTimeMapper mapper,
            ILeaveTimeRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<Guid> Execute(CreateLeaveTimeRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var createdBy = _httpContextAccessor.HttpContext.GetUserId();
            var dbLeaveTime = _mapper.Map(request, createdBy);

            return new OperationResultResponse<Guid>
            {
                Body = _repository.Add(dbLeaveTime),
                Status = OperationResultStatusType.FullSuccess
            };
        }
    }
}
