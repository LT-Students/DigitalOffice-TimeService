using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    public UserInfo Map(UserData userData, PositionUserData positionUserData)
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
