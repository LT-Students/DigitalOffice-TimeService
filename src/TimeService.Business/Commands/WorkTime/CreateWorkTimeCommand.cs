using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly ICreateWorkTimeRequestValidator _validator;
        private readonly IDbWorkTimeMapper _mapper;
        private readonly IWorkTimeRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateWorkTimeCommand(
            ICreateWorkTimeRequestValidator validator,
            IDbWorkTimeMapper mapper,
            IWorkTimeRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid Execute(CreateWorkTimeRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var createdBy = _httpContextAccessor.HttpContext.GetUserId();
            var dbWorkTime = _mapper.Map(request, createdBy);

            return _repository.CreateWorkTime(dbWorkTime);
        }
    }
}
