using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit
{
  public class FindWorkTimeMonthLimitCommand : IFindWorkTimeMonthLimitCommand
  {
    private readonly IBaseFindFilterValidator _baseFindValidator;
    private readonly IWorkTimeMonthLimitRepository _repository;
    private readonly IWorkTimeMonthLimitInfoMapper _mapper;
    private readonly IResponseCreater _responseCreator;

    public FindWorkTimeMonthLimitCommand(
      IBaseFindFilterValidator baseFindValidator,
      IWorkTimeMonthLimitRepository repository,
      IWorkTimeMonthLimitInfoMapper mapper,
      IResponseCreater responseCreator)
    {
      _baseFindValidator = baseFindValidator;
      _repository = repository;
      _mapper = mapper;
      _responseCreator = responseCreator;
    }

    public async Task<FindResultResponse<WorkTimeMonthLimitInfo>> ExecuteAsync(FindWorkTimeMonthLimitsFilter filter)
    {
      if (!_baseFindValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<WorkTimeMonthLimitInfo>(
          HttpStatusCode.BadRequest,
          errors);
      }

      (List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount) = await _repository.FindAsync(filter);

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body = dbWorkTimeMonthLimit.Select(_mapper.Map).ToList(),
        TotalCount = totalCount,
        Errors = new()
      };
    }
  }
}
