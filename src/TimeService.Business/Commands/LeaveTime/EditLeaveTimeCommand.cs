using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
    public class EditLeaveTimeCommand : IEditLeaveTimeCommand
    {
        private readonly IEditLeaveTimeRequestValidator _validator;
        private readonly ILeaveTimeRepository _repository;
        private readonly IPatchDbLeaveTimeMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private void ValidateOverlapping(DbLeaveTime oldLeaveTime, Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
        {
            List<DbLeaveTime> leaveTimes = null;

            Operation<EditLeaveTimeRequest> startTimeOperation = request.Operations.FirstOrDefault(
                o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
            if (startTimeOperation != null)
            {
                DateTime startTime = DateTime.Parse(startTimeOperation.value.ToString());

                if (oldLeaveTime.EndTime <= startTime)
                {
                    throw new BadRequestException("Start time should be less than end.");
                }

                leaveTimes = _repository.Find(new FindLeaveTimesFilter { UserId = oldLeaveTime.UserId }, 0, int.MaxValue, out _);

                if (!leaveTimes.All(t => t.EndTime <= startTime || t.StartTime >= startTime))
                {
                    throw new BadRequestException("New LeaveTime should not overlap with old ones.");
                }
            }

            Operation<EditLeaveTimeRequest> endTimeOperation = request.Operations.FirstOrDefault(
                o => o.path.EndsWith(nameof(EditWorkTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));
            if (endTimeOperation != null)
            {
                DateTime endTime = DateTime.Parse(endTimeOperation.value.ToString());

                if (oldLeaveTime.StartTime >= endTime)
                {
                    throw new BadRequestException("End time should be greater than start.");
                }

                leaveTimes ??= _repository.Find(new FindLeaveTimesFilter { UserId = oldLeaveTime.UserId }, 0, int.MaxValue, out _);

                if (!leaveTimes.All(t => t.StartTime >= endTime || t.EndTime <= endTime && t.StartTime <= endTime))
                {
                    throw new BadRequestException("New LeaveTime should not overlap with old ones.");
                }
            }

            if (startTimeOperation != null
                && endTimeOperation != null)
            {
                DateTime startTime = DateTime.Parse(startTimeOperation.value.ToString());
                DateTime endTime = DateTime.Parse(endTimeOperation.value.ToString());

                if (!leaveTimes.All(t => t.StartTime >= endTime || t.EndTime <= startTime))
                {
                    throw new BadRequestException("New LeaveTime should not overlap with old ones.");
                }
            }
        }

        public EditLeaveTimeCommand(
            IEditLeaveTimeRequestValidator validator,
            ILeaveTimeRepository repository,
            IPatchDbLeaveTimeMapper mapper,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<bool> Execute(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
        {
            var oldLeaveTime = _repository.Get(leaveTimeId);

            var isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldLeaveTime.UserId;
            if (!isOwner && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            ValidateOverlapping(oldLeaveTime, leaveTimeId, request);

            return new OperationResultResponse<bool>
            {
                Body = _repository.Edit(oldLeaveTime, _mapper.Map(request)),
                Status = OperationResultStatusType.FullSuccess,
                Errors = new()
            };
        }
    }
}
