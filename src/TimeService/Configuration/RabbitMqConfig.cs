using LT.DigitalOffice.Kernel.Configurations;

namespace LT.DigitalOffice.TimeService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        public string AuthenticationServiceValidationUrl { get; set; }
    }
}
