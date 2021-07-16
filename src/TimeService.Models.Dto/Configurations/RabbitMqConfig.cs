using LT.DigitalOffice.Kernel.Configurations;

namespace LT.DigitalOffice.TimeService.Models.Dto.Configurations
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        public string AuthenticationServiceValidationUrl { get; set; }
    }
}
