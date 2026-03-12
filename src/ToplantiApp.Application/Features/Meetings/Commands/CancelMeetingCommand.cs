using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record CancelMeetingCommand(int Id) : IRequest<Response>;

public class CancelMeetingCommandHandler : IRequestHandler<CancelMeetingCommand, Response>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMailService _mailService;
    private readonly ICurrentUserProvider _currentUser;

    public CancelMeetingCommandHandler(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, IMailService mailService, ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
        _mailService = mailService;
        _currentUser = currentUser;
    }

    public async Task<Response> Handle(CancelMeetingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        if (meeting.CreatedByUserId != userId)
            throw new ForbiddenException("Bu toplantiyi iptal etme yetkiniz yok.");

        if (meeting.Status == MeetingStatus.Cancelled)
            throw new AppException("Bu toplanti zaten iptal edilmis.");

        var now = DateTime.UtcNow;
        if (meeting.StartDate <= now)
            throw new AppException("Baslamis veya bitmis toplantilar iptal edilemez.");

        meeting.Status = MeetingStatus.Cancelled;
        meeting.CancelledAt = now;

        _meetingRepository.Update(meeting);
        await _unitOfWork.SaveChangesAsync();

        foreach (var participant in meeting.Participants)
        {
            _ = _mailService.SendMeetingNotificationAsync(
                participant.Email,
                participant.FullName,
                meeting,
                $"Toplanti Iptal Edildi: {meeting.Name}",
                "Bu toplanti iptal edilmistir.");
        }

        return Response.Ok("Toplanti iptal edildi.");
    }
}
