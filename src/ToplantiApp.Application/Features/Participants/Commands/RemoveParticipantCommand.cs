using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Commands;

public record RemoveParticipantCommand(int MeetingId, int ParticipantId, int UserId) : IRequest<Unit>;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveParticipantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantidan katilimci cikarma yetkiniz yok.");

        var participant = await _unitOfWork.MeetingParticipants.GetByIdAsync(request.ParticipantId)
            ?? throw new NotFoundException("Katilimci", request.ParticipantId);

        _unitOfWork.MeetingParticipants.Delete(participant);
        await _unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}
