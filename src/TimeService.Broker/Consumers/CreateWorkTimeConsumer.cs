using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Time;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using MassTransit;

namespace LT.DigitalOffice.TimeService.Broker.Consumers
{
  public class CreateWorkTimeConsumer : IConsumer<ICreateWorkTimeRequest>
    {
        private readonly IWorkTimeRepository _workTimeRepository;

        private async Task<bool> CreateWorkTime(ICreateWorkTimeRequest request)
        {
            DateTime timeNow = DateTime.UtcNow;

            foreach(Guid userId in request.UserIds)
            {
                await _workTimeRepository.CreateAsync(
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    ProjectId = request.ProjectId,
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

        public async Task Consume(ConsumeContext<ICreateWorkTimeRequest> context)
        {
            object result = OperationResultWrapper.CreateResponse(CreateWorkTime, context.Message);

            await context.RespondAsync<IOperationResult<bool>>(result);
        }
    }
}
