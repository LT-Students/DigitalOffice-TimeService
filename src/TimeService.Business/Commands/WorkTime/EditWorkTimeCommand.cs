using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IEditWorkTimeRequestValidator _validator;
        private readonly IWorkTimeRepository _repository;
        private readonly IDbWorkTimeMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditWorkTimeCommand(
            IEditWorkTimeRequestValidator validator,
            IWorkTimeRepository repository,
            IDbWorkTimeMapper mapper,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Execute(EditWorkTimeRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var oldDbWorkTime = _repository.GetWorkTime(request.Id);
            var isAuthor = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.CreatedBy;
            if (!_accessValidator.IsAdmin() && !isAuthor)
            {
                throw new ForbiddenException("Not enough rights.");
            }

            return _repository.EditWorkTime(_mapper.Map(request, oldDbWorkTime));
        }
    }
}
