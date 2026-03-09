using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record CancelMeetingCommand(int Id, int UserId) : IRequest<Unit>;

public class CancelMeetingCommandHandler : IRequestHandler<CancelMeetingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMailService _mailService;

    public CancelMeetingCommandHandler(IUnitOfWork unitOfWork, IMailService mailService)
    {
        _unitOfWork = unitOfWork;
        _mailService = mailService;
    }

    public async Task<Unit> Handle(CancelMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantiyi iptal etme yetkiniz yok.");

        if (meeting.Status == MeetingStatus.Cancelled)
            throw new AppException("Bu toplanti zaten iptal edilmis.");

        meeting.Status = MeetingStatus.Cancelled;
        meeting.CancelledAt = DateTime.UtcNow;

        _unitOfWork.Meetings.Update(meeting);
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

        return Unit.Value;
    }
}
