using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Models.Broker.Requests.Project;

namespace LT.DigitalOffice.TimeService.Models.Dto.Configurations
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        public string AuthenticationServiceValidationUrl { get; set; }

        public string CreateWorkTimeEndpoint { get; set; }

        [AutoInjectRequest(typeof(IGetUserProjectsRequest))]
        public string GetProjectIdsEndpoint { get; set; }

        [AutoInjectRequest(typeof(IFindProjectsRequest))]
        public string FindProjectsEndpoint { get; set; }

        [AutoInjectRequest(typeof(IGetProjectsUsersRequest))]
        public string GetProjectsUsersEndpoint { get; set; }
    }
}
