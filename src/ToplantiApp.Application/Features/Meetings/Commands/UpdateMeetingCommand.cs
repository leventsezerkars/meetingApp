using AutoMapper;
using FluentValidation;
using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record UpdateMeetingCommand(int Id, UpdateMeetingDto Data) : IRequest<Response<MeetingDto>>;

public class UpdateMeetingCommandValidator : AbstractValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Data.EndDate).GreaterThan(x => x.Data.StartDate);
    }
}

public class UpdateMeetingCommandHandler : IRequestHandler<UpdateMeetingCommand, Response<MeetingDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserProvider _currentUser;

    public UpdateMeetingCommandHandler(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Response<MeetingDto>> Handle(UpdateMeetingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        if (meeting.CreatedByUserId != userId)
            throw new ForbiddenException("Bu toplantiyi duzenleme yetkiniz yok.");

        meeting.Name = request.Data.Name;
        meeting.Description = request.Data.Description;
        meeting.StartDate = request.Data.StartDate;
        meeting.EndDate = request.Data.EndDate;

        _meetingRepository.Update(meeting);
        await _unitOfWork.SaveChangesAsync();

        return Response<MeetingDto>.Ok(_mapper.Map<MeetingDto>(meeting));
    }
}
