using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    private readonly IImageMapper _imageMapper;

    public UserInfoMapper(ImageMapper imageMapper)
    {
      _imageMapper = imageMapper;
    }

    public UserInfo Map(UserData userData, ImageData imageData = null)
    {
      if (userData == null)
      {
        return null;
      }

      return new()
      {
        Id = userData.Id,
        FirstName = userData.FirstName,
        MiddleName = userData.MiddleName,
        LastName = userData.LastName,
        IsActive = userData.IsActive,
        Image = imageData is null
          ? null
          : _imageMapper.Map(imageData)
      };
    }
  }
}
