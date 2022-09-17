using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Models.Broker.Requests.Email;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class EmailService : IEmailService
  {
    private readonly IRequestClient<ISendEmailRequest> _rcSendEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
      IRequestClient<ISendEmailRequest> rcSendEmail,
      ILogger<EmailService> logger)
    {
      _rcSendEmail = rcSendEmail;
      _logger = logger;
    }

    public async Task SendAsync(string email, string subject, string text, List<string> errors = null)
    {
      if (!await RequestHandler.ProcessRequest<ISendEmailRequest, bool>(
        _rcSendEmail,
        ISendEmailRequest.CreateObj(
          receiver: email,
          subject: subject,
          text: text),
        errors,
        _logger))
      {
        _logger.LogError(
          "Letter not sent to email '{Email}'",
          email);

        errors?.Add($"Can not send email to '{email}'. Email placed in resend queue and will be resent in 1 hour.");
      }
    }
  }
}
