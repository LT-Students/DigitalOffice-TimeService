using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentValidation;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
  {
    private bool IsMonthValid(int month, sbyte offset)
    {
      DateTime dateTimeNow = DateTime.UtcNow.AddHours(offset);

      if (month == dateTimeNow.Month
        || (dateTimeNow.Day <= 5 && dateTimeNow.AddMonths(-1).Month == month))
      {
        return true;
      }

      return false;
    }

    private bool IsYearValid(int year, int month, sbyte offset)
    {
      DateTime dateTimeNow = DateTime.UtcNow.AddHours(offset);

      if (dateTimeNow.Year == year
        || (dateTimeNow.AddMonths(-1).Year == year && month == 1))
      {
        return true;
      }

      return false;
    }

    public CreateWorkTimeRequestValidator(
      IUserService userService,
      IHttpContextAccessor httpContextAccessor,
      IWorkTimeRepository workTimeRepository)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

      RuleFor(_ => httpContextAccessor.HttpContext.GetUserId())
        .MustAsync(async (userId, _) => (await userService.CheckUsersExistenceAsync(new List<Guid> { userId }))?.Count == 1)
        .WithMessage(WorkTimeValidationResource.UserDoesNotExist);

      RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Description is too long.");

      RuleFor(request => request)
        .Must(request => request.Offset >= -12 && request.Offset <= 12).WithMessage(WorkTimeValidationResource.IncorrectOffset)
        .Must(request => IsMonthValid(request.Month, request.Offset)).WithMessage(WorkTimeValidationResource.IncorrectMonth)
        .Must(request => IsYearValid(request.Year, request.Month, request.Offset)).WithMessage(WorkTimeValidationResource.IncorrectYear)
        .MustAsync(async (x, _) =>
          !await workTimeRepository.DoesEmptyWorkTimeExistAsync(httpContextAccessor.HttpContext.GetUserId(), x.Month, x.Year))
        .WithMessage(WorkTimeValidationResource.WorkTimeAlreadyExists);
    }
  }
}
