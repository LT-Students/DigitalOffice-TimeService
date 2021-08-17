using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;

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

        [AutoInjectRequest(typeof(ICheckUsersExistence))]
        public string CheckUsersExistenceEndpoint { get; set; }

        [AutoInjectRequest(typeof(IGetProjectUsersRequest))]
        public string GetProjectUsersEndpoint { get; set; }

        [AutoInjectRequest(typeof(IGetUsersDataRequest))]
        public string GetUsersDataEndpoint { get; set; }
    }
}
