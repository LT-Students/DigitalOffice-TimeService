using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
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
        Image =  new()
        {
          ImageId = imageData.ImageId,
          ParentId = imageData.ParentId,
          Content = imageData.Content,
          Extension = imageData.Extension,
          Name = imageData.Name
        }
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
