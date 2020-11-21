using LT.DigitalOffice.Kernel.Broker;

namespace LT.DigitalOffice.TimeManagementService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string AuthenticationServiceValidationUrl { get; set; }
    }
}
