using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob
{
    public class CreateWorkTimeDayJobCommand : ICreateWorkTimeDayJobCommand
    {
        private readonly ICreateWorkTimeDayJobRequestValidator _validator;
        private readonly IAccessValidator _accessValidator;
        private readonly IDbWorkTimeDayJobMapper _mapper;
        private readonly IWorkTimeDayJobRepository _repository;
        private readonly IWorkTimeRepository _workTimeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateWorkTimeDayJobCommand(
            ICreateWorkTimeDayJobRequestValidator validator,
            IAccessValidator accessValidator,
            IDbWorkTimeDayJobMapper mapper,
            IWorkTimeDayJobRepository repository,
            IWorkTimeRepository workTimeRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _accessValidator = accessValidator;
            _mapper = mapper;
            _repository = repository;
            _workTimeRepository = workTimeRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<Guid> Execute(CreateWorkTimeDayJobRequest request)
        {
            Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

            DbWorkTime workTime = _workTimeRepository.Get(request.WorkTimeId);

            if (authorId != workTime.UserId
                && !_accessValidator.IsAdmin()
                && !_accessValidator.HasRights(Rights.AddEditRemoveTime))
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            return new OperationResultResponse<Guid>
            {
                Status = OperationResultStatusType.FullSuccess,
                Body = _repository.Create(_mapper.Map(request)),
                Errors = new()
            };
        }
    }
}
