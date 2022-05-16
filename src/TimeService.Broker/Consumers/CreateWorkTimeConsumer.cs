using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Time;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using MassTransit;

namespace LT.DigitalOffice.TimeService.Broker.Consumers
{
  public class CreateWorkTimeConsumer : IConsumer<ICreateWorkTimePublish>
    {
        private readonly IWorkTimeRepository _workTimeRepository;

        private bool CreateWorkTime(ICreateWorkTimePublish publish)
        {
            DateTime timeNow = DateTime.UtcNow;

            foreach(Guid userId in publish.UserIds)
            {
                _workTimeRepository.CreateAsync(
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    ProjectId = publish.ProjectId,
                    UserId = userId,
                    Month = timeNow.Month,
                    Year = timeNow.Year
                });
            }

            return true;
        }

        public CreateWorkTimeConsumer(IWorkTimeRepository workTimeRepository)
        {
            _workTimeRepository = workTimeRepository;
        }

        public async Task Consume(ConsumeContext<ICreateWorkTimePublish> context)
        {
            object result = OperationResultWrapper.CreateResponse(CreateWorkTime, context.Message);

            await context.RespondAsync<IOperationResult<bool>>(result);
        }
    }
}
