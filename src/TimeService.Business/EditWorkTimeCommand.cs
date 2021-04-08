using FluentValidation;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IEditWorkTimeRequestValidator _validator;
        private readonly IWorkTimeRepository _repository;
        private readonly IDbWorkTimeMapper _mapper;

        public EditWorkTimeCommand(
            IEditWorkTimeRequestValidator validator,
            IWorkTimeRepository repository,
            IDbWorkTimeMapper mapper)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
        }

        public bool Execute(EditWorkTimeRequest request)
        {
            var validationResult = _validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            return _repository.EditWorkTime(_mapper.Map(request));
        }
    }
}
