using LT.DigitalOffice.Kernel.Broker;

namespace LT.DigitalOffice.TimeManagementService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string AuthenticationServiceValidationUrl { get; set; }
        public string UserServiceUrl { get; set; }
        public string ProjectService_ProjectUserUrl { get; set; }
    }
}
