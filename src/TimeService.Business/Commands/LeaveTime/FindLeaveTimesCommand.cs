using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class FindLeaveTimesCommand : IFindLeaveTimesCommand
  {
    private readonly IBaseFindFilterValidator _validator;
    private readonly ILeaveTimeResponseMapper _leaveTimeResponseMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly ILeaveTimeRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IResponseCreator _responsCreator;

    #region private methods

    private async Task<List<UserData>> GetUsersData(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      return await _userService.GetUsersDataAsync(usersIds, errors);
    }

    private async Task<List<CompanyData>> GetCompaniesAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      return await _companyService.GetCompaniesDataAsync(usersIds, errors);
    }

    #endregion

    public FindLeaveTimesCommand(
      IBaseFindFilterValidator validator,
      ILeaveTimeResponseMapper leaveTimeResponseMapper,
      IUserInfoMapper userInfoMapper,
      ILeaveTimeRepository repository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IUserService userService,
      ICompanyService companyService,
      IResponseCreator responseCreator)
    {
      _validator = validator;
      _leaveTimeResponseMapper = leaveTimeResponseMapper;
      _userInfoMapper = userInfoMapper;
      _repository = repository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _userService = userService;
      _companyService = companyService;
      _responsCreator = responseCreator;
    }

    public async Task<FindResultResponse<LeaveTimeResponse>> ExecuteAsync(FindLeaveTimesFilter filter)
    {
      bool isAuthor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isAuthor && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responsCreator.CreateFailureFindResponse<LeaveTimeResponse>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responsCreator.CreateFailureFindResponse<LeaveTimeResponse>(HttpStatusCode.BadRequest, errors);
      }

      (List<DbLeaveTime> dbLeaveTimes, int totalCount) = await _repository.FindAsync(filter);

      List<Guid> usersIds = dbLeaveTimes.Select(lt => lt.UserId).ToList();

      Task<List<UserData>> usersTask = GetUsersData(usersIds, errors);
      Task<List<CompanyData>> companiesTask = GetCompaniesAsync(usersIds, errors);

      await Task.WhenAll(usersTask, companiesTask);

      List<CompanyUserData> companies = (await companiesTask)?.SelectMany(p => p.Users).ToList();

      List<UserInfo> users = (await usersTask)
        ?.Select(u => _userInfoMapper.Map(u, companies?.FirstOrDefault(p => p.UserId == u.Id))).ToList();

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        TotalCount = totalCount,
        Body = dbLeaveTimes.Select(lt => _leaveTimeResponseMapper.Map(lt, users?.FirstOrDefault(u => u.Id == lt.UserId))).ToList(),
        Errors = errors
      };
    }
  }
}
