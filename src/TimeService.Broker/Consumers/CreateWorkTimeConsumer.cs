using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Time;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using MassTransit;

namespace LT.DigitalOffice.TimeService.Broker.Consumers
{
  public class CreateWorkTimeConsumer : IConsumer<ICreateWorkTimePublish>
  {
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IDbWorkTimeMapper _mapper;

    private async Task CreateWorkTime(ICreateWorkTimePublish publish)
    {
      IEnumerable<Guid> existingUsersIds = (await _workTimeRepository.GetAsync(
        usersIds: publish.UserIds,
        projectsIds: new() { publish.ProjectId },
        year: DateTime.Now.Year,
        month: DateTime.Now.Month))?.Select(wt => wt.UserId);

      if (existingUsersIds is null || !existingUsersIds.Any())
      {
        await _workTimeRepository.CreateAsync(
          publish.UserIds.Select(u => _mapper.Map(u, publish.ProjectId)).ToList());
      }
      else
      {
        await _workTimeRepository.CreateAsync(
          publish.UserIds.Except(existingUsersIds).Select(u => _mapper.Map(u, publish.ProjectId)).ToList());
      }
    }

    public CreateWorkTimeConsumer(
      IWorkTimeRepository workTimeRepository,
      IDbWorkTimeMapper mapper)
    {
      _workTimeRepository = workTimeRepository;
      _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<ICreateWorkTimePublish> context)
    {
      await CreateWorkTime(context.Message);
    }
  }
}
