using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class ProjectInfoMapper : IProjectInfoMapper
    {
        public ProjectInfo Map(ProjectData project)
        {
            if (project == null)
            {
                return null;
            }

            return new ProjectInfo
            {
                Id = project.Id,
                DepartmentId = project.DepartmentId,
                Name = project.Name,
                ShortDescription = project.ShortDescription,
                ShortName = project.ShortName,
                Status = project.Status
            };
        }
    }
}
