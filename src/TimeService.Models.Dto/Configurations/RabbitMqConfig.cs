using LT.DigitalOffice.Kernel.BrokerSupport.Attributes;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Requests.Email;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.TextTemplate;
using LT.DigitalOffice.Models.Broker.Requests.User;

namespace LT.DigitalOffice.TimeService.Models.Dto.Configurations
{
  public class RabbitMqConfig : BaseRabbitMqConfig
  {
    public string AuthenticationServiceValidationUrl { get; set; }
    public string CreateWorkTimeEndpoint { get; set; }

    // companies
    [AutoInjectRequest(typeof(IGetCompaniesRequest))]
    public string GetCompaniesEndpoint { get; set; }

    // project

    [AutoInjectRequest(typeof(IGetProjectsUsersRequest))]
    public string GetProjectsUsersEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetProjectsRequest))]
    public string GetProjectsEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetProjectUserRoleRequest))]
    public string GetProjectUserRoleEndpoint { get; set; }

    // user

    [AutoInjectRequest(typeof(ICheckUsersExistence))]
    public string CheckUsersExistenceEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetUsersDataRequest))]
    public string GetUsersDataEndpoint { get; set; }

    [AutoInjectRequest(typeof(IFilteredUsersDataRequest))]
    public string FilterUsersDataEndpoint { get; set; }

    // department

    [AutoInjectRequest(typeof(IGetDepartmentsUsersRequest))]
    public string GetDepartmentsUsersEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetDepartmentsRequest))]
    public string GetDepartmentsEndpoint { get; set; }

    [AutoInjectRequest(typeof(IFilterDepartmentsRequest))]
    public string FilterDepartmentUsersEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetDepartmentUserRoleRequest))]
    public string GetDepartmentUserRoleEndpoint { get; set; }

    // image
    [AutoInjectRequest(typeof(IGetImagesRequest))]
    public string GetImagesEndpoint { get; set; }

    // position
    [AutoInjectRequest(typeof(IGetPositionsRequest))]
    public string GetPositionsEndpoint { get; set; }

    //Email
    [AutoInjectRequest(typeof(ISendEmailRequest))]
    public string SendEmailEndpoint { get; set; }

    //TextTemplate
    [AutoInjectRequest(typeof(IGetTextTemplateRequest))]
    public string GetTextTemplateEndpoint { get; set; }
  }
}
