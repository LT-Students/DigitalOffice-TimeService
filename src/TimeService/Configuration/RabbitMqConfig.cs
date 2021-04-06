using LT.DigitalOffice.Kernel.Broker;

namespace LT.DigitalOffice.TimeService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string AuthenticationServiceValidationUrl { get; set; }
    }
}
