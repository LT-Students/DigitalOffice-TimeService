using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Responses.Position;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class PositionService : IPositionService
  {
    private readonly ILogger<PositionService> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IRequestClient<IGetPositionsRequest> _rcRequestClient;

    public PositionService(
      ILogger<PositionService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IGetPositionsRequest> rcRequestClient)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcRequestClient = rcRequestClient;
    }

    public async Task<List<PositionData>> GetPositionsAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      object request = IGetPositionsRequest.CreateObj(usersIds);

      List<PositionData> positionsData =
        await _globalCache.GetAsync<List<PositionData>>(Cache.Positions, usersIds.GetRedisCacheKey(request.GetBasicProperties()));

      if (positionsData is null)
      {
        positionsData =
          (await RequestHandler.ProcessRequest<IGetPositionsRequest, IGetPositionsResponse>(
            _rcRequestClient,
            request,
            errors,
            _logger))
          ?.Positions;
      }

      return positionsData;
    }
  }
}
