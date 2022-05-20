using System.Threading.Tasks;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Time;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using MassTransit;

namespace LT.DigitalOffice.TimeService.Broker.Consumers
{
  public class CreateWorkTimeConsumer : IConsumer<ICreateWorkTimePublish>
  {
    private readonly IWorkTimeRepository _workTimeRepository;

    private async Task CreateWorkTime(ICreateWorkTimePublish publish)
    {
      await _workTimeRepository.CreateAsync(publish.UserIds, publish.ProjectId);
    }

    public CreateWorkTimeConsumer(IWorkTimeRepository workTimeRepository)
    {
      _workTimeRepository = workTimeRepository;
    }

    public async Task Consume(ConsumeContext<ICreateWorkTimePublish> context)
    {
      await CreateWorkTime(context.Message);
    }
  }
}
