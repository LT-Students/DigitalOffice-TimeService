using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class ImageService : IImageService
  {
    private readonly ILogger<ImageService> _logger;
    private readonly IRequestClient<IGetImagesRequest> _rcRequestClient;

    public ImageService(
      ILogger<ImageService> logger,
      IRequestClient<IGetImagesRequest> rcRequestClient)
    {
      _logger = logger;
      _rcRequestClient = rcRequestClient;
    }

    public async Task<List<ImageData>> GetUsersImagesAsync(List<Guid> imagesIds, List<string> errors)
    {
      if (imagesIds is null || !imagesIds.Any())
      {
        return null;
      }

      List<ImageData> imagesData =
        (await RequestHandler.ProcessRequest<IGetImagesRequest, IGetImagesResponse>(
          _rcRequestClient,
          IGetImagesRequest.CreateObj(imagesIds, ImageSource.User),
          errors,
          _logger))
        ?.ImagesData;

      return imagesData;
    }
  }
}
