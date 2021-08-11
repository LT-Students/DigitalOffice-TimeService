using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob
{
    public class EditWorkTimeDayJobCommand : IEditWorkTimeDayJobCommand
    {
        private readonly IEditWorkTimeDayJobRequestValidator _validator;
        private readonly IAccessValidator _accessValidator;
        private readonly IPatchDbWorkTimeDayJobMapper _mapper;
        private readonly IWorkTimeDayJobRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditWorkTimeDayJobCommand(
            IEditWorkTimeDayJobRequestValidator validator,
            IAccessValidator accessValidator,
            IPatchDbWorkTimeDayJobMapper mapper,
            IWorkTimeDayJobRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _accessValidator = accessValidator;
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<bool> Execute(Guid workTimeDayJobId, JsonPatchDocument<EditWorkTimeDayJobRequest> request)
        {
            DbWorkTimeDayJob dayJob = _repository.Get(workTimeDayJobId, true);
            Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

            if (dayJob.WorkTime.UserId != authorId
                && !_accessValidator.IsAdmin()
                && !_accessValidator.HasRights(Rights.AddEditRemoveTime))
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            return new OperationResultResponse<bool>
            {
                Status = OperationResultStatusType.FullSuccess,
                Body = _repository.Edit(workTimeDayJobId, _mapper.Map(request)),
                Errors = new()
            };
        }
    }
}
