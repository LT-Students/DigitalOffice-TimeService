using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class ImageMapper : IImageMapper
  {
    public ImageInfo Map(ImageData imageData)
    {
      if (imageData is null)
      {
        return null;
      }

      return new()
      {
        ImageId = imageData.ImageId,
        ParentId = imageData.ParentId,
        Content = imageData.Content,
        Extension = imageData.Extension,
        Name = imageData.Name
      };
    }
  }
}
