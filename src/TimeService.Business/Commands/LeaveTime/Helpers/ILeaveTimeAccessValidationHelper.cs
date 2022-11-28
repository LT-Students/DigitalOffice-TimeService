using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers
{
  [AutoInject]
  public interface ILeaveTimeAccessValidationHelper
  {
    Task<bool> HasRightsAsync(Guid? ltOwnerId);
  }
}
