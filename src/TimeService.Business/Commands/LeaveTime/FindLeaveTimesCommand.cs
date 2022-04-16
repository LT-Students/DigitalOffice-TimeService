using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IRequestClient<IGetCompaniesRequest> _rcGetCompanies;
    private readonly ILogger<FindLeaveTimesCommand> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IResponseCreator _responsCreator;

    #region private methods

    private async Task<List<UserData>> GetUsersData(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      List<UserData> usersFromCache = await _globalCache.GetAsync<List<UserData>>(Cache.Users, usersIds.GetRedisCacheHashCode());

      if (usersFromCache != null)
      {
        _logger.LogInformation("UserDatas were taken from the cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return usersFromCache;
      }

      return await GetUsersDataFromBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataFromBrokerAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      string message = "Cannot get users data. Please try again later.";
      string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", usersIds)}'.";

      try
      {
        Response<IOperationResult<IGetUsersDataResponse>> response =
          await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
            IGetUsersDataRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UserDatas were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.UsersData;
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add(message);

      return null;
    }

    private async Task<List<CompanyData>> GetCompaniesAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      List<CompanyData> companies = await _globalCache.GetAsync<List<CompanyData>>(Cache.Companies, usersIds.GetRedisCacheHashCode());

      if (companies != null)
      {
        _logger.LogInformation("Companies for users were taken from cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return companies;
      }

      return await GetCompaniesThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<CompanyData>> GetCompaniesThroughBrokerAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      const string errorMessage = "Can not get companies info. Please try again later.";

      try
      {
        Response<IOperationResult<IGetCompaniesResponse>> response = await _rcGetCompanies
          .GetResponse<IOperationResult<IGetCompaniesResponse>>(
            IGetCompaniesRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Companies were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.Companies;
        }
        else
        {
          _logger.LogWarning("Errors while getting companies info. Reason: {Errors}",
            string.Join('\n', response.Message.Errors));
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, errorMessage);
      }

      errors.Add(errorMessage);

      return null;
    }

    #endregion

    public FindLeaveTimesCommand(
      IBaseFindFilterValidator validator,
      ILeaveTimeResponseMapper leaveTimeResponseMapper,
      IUserInfoMapper userInfoMapper,
      ILeaveTimeRepository repository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IRequestClient<IGetCompaniesRequest> rcGetCompanies,
      ILogger<FindLeaveTimesCommand> logger,
      IGlobalCacheRepository globalCache,
      IResponseCreator responseCreator)
    {
      _validator = validator;
      _leaveTimeResponseMapper = leaveTimeResponseMapper;
      _userInfoMapper = userInfoMapper;
      _repository = repository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _rcGetUsers = rcGetUsers;
      _rcGetCompanies = rcGetCompanies;
      _logger = logger;
      _globalCache = globalCache;
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
