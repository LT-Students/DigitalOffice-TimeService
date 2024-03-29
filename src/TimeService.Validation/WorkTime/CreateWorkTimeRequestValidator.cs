﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentValidation;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Resources;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
  {
    private const int LastDayToEditPreviousMonth = 10;

    private bool IsDateValid(int month, int year)
    {
      DateTime dateTimeNow = DateTime.UtcNow;

      DateTime thisMonthFirstDay = new DateTime(dateTimeNow.Year, dateTimeNow.Month, 1);
      DateTime workTimeMonthFirstDay = new DateTime(year, month, 1);

      return thisMonthFirstDay == workTimeMonthFirstDay || (thisMonthFirstDay.AddMonths(-1) == workTimeMonthFirstDay && dateTimeNow.Day <= LastDayToEditPreviousMonth);
    }

    public CreateWorkTimeRequestValidator(
      IUserService userService,
      IHttpContextAccessor httpContextAccessor,
      IWorkTimeRepository workTimeRepository)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

      RuleFor(_ => httpContextAccessor.HttpContext.GetUserId())
        .MustAsync(async (userId, _) => (await userService.CheckUsersExistenceAsync(new List<Guid> { userId }))?.Count == 1)
        .WithMessage(WorkTimeValidationResource.UserDoesNotExist);

      RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage($"{nameof(CreateWorkTimeRequest.Description)} {WorkTimeValidationResource.LongPropertyValue}");

      RuleFor(request => request)
        .Must(request => request.Offset >= -12 && request.Offset <= 12).WithMessage(WorkTimeValidationResource.OffsetIsIncorrect)
        .Must(request => IsDateValid(month: request.Month, year: request.Year)).WithMessage(WorkTimeValidationResource.DateIsIncorrect)
        .MustAsync(async (x, _) =>
          !await workTimeRepository.DoesEmptyWorkTimeExistAsync(httpContextAccessor.HttpContext.GetUserId(), x.Month, x.Year))
        .WithMessage(WorkTimeValidationResource.WorkTimeAlreadyExists);
    }
  }
}
