using LT.DigitalOffice.Kernel.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IEmailService
  {
    Task SendAsync(string email, string subject, string text, List<string> errors = null);
  }
}
