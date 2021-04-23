using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
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
            _validator.ValidateAndThrowCustom(request);

            return _repository.EditWorkTime(_mapper.Map(request));
        }
    }
}
