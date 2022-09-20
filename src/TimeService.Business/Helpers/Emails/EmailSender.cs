using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Helpers.TextHandlers.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Responses.TextTemplate;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Emails
{
  public class EmailSender
  {
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ITextTemplateService _textTemplateService;
    private readonly ITextTemplateParser _parser;

    private async Task NotifyAsync(
      UserData user,
      string locale)
    {
      IGetTextTemplateResponse textTemplate = await _textTemplateService
        .GetAsync(TemplateType.EmptyUserWorktimes, locale);

      if (textTemplate is null)
      {
        return;
      }

      string parsedText = _parser.Parse(
        new Dictionary<string, string> { 
          { "FirstName", user.FirstName },
          { "LastName", user.LastName },
          { "FirstLastMonth", new DateTime(year: DateTime.UtcNow.Year, month: DateTime.UtcNow.Month, day: 1).ToShortDateString() },
          { "LastLastMonth", new DateTime(year: DateTime.UtcNow.Year,
            month: DateTime.UtcNow.Month, 
            day: DateTime.DaysInMonth(DateTime.UtcNow.Year, month: DateTime.UtcNow.Month)).ToShortDateString() },
          { "LastCurrentMonth", new DateTime(year: DateTime.UtcNow.Year, 
            month: DateTime.UtcNow.Month + 1, 
            day: DateTime.DaysInMonth(DateTime.UtcNow.Year, month: DateTime.UtcNow.Month + 1)).ToShortDateString() }
        },
        textTemplate.Text);

      await _emailService.SendAsync(user.Email, textTemplate.Subject, parsedText);
    }

    private async Task<bool> ExecuteAsync()
    {
      List<Guid> usersIds = await _workTimeRepository.GetUsersWithNullWorktimeAsync();

      if (usersIds is null || !usersIds.Any())
      {
        return false;
      }

      List<UserData> users = await _userService.GetUsersDataAsync(
        usersIds: usersIds,
        errors: null,
        includeBaseEmail: true);

      if (users is null || !users.Any())
      {
        return false;
      }

      foreach (UserData user in users)
      {
        if (user.IsActive)
        {
          await NotifyAsync(user, "ru");
        }
      }

      return true;
    }

    public EmailSender(
      IWorkTimeRepository workTimeRepository,
      IUserService userService,
      IEmailService emailService,
      ITextTemplateService textTemplateService,
      ITextTemplateParser parser)
    {
      _workTimeRepository = workTimeRepository;
      _userService = userService;
      _emailService = emailService;
      _textTemplateService = textTemplateService;
      _parser = parser;
    }

    public void Start()
    {
      Task.Run(async () =>
      {
        while (true)
        {
          if (DateTime.UtcNow.Day == 20 && DateTime.UtcNow.Hour == 23)
          {
            await ExecuteAsync();
          }

          Thread.Sleep(3600000);
        }
      });
    }
  }
}
