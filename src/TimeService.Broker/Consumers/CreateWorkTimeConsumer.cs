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
      await _workTimeRepository.CreateAsync(
        publish.UserIds.Select(u => _mapper.Map(u, publish.ProjectId)).ToList());
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
