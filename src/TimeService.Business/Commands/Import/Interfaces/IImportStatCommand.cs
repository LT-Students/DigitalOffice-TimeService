using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;

namespace LT.DigitalOffice.TimeService.Business.Commands.Import.Interfaces
{
  [AutoInject]
  public interface IImportStatCommand
  {
    Task<OperationResultResponse<byte[]>> Execute(ImportStatFilter filter);
  }
}
