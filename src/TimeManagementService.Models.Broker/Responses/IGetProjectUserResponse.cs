using System;

namespace LT.DigitalOffice.Broker.Responses
{
    public interface IGetProjectUserResponse
    {
        Guid Id { get; }
        bool IsActive { get; }
    }
}
