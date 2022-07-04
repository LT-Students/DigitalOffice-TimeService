using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
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

    public UserInfo Map(UserData userData, CompanyUserData companyUserData, ImageData imageData)
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
        Image =  _imageMapper.Map(imageData)
      };
    }

    public UserInfo Map(UserData userData, CompanyUserData companyUserData)
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
        IsActive = userData.IsActive
      };
    }
  }
}
