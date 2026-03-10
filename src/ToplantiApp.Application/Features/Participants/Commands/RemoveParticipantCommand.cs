using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Commands;

public record RemoveParticipantCommand(int MeetingId, int ParticipantId, int UserId) : IRequest<Unit>;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Unit>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveParticipantCommandHandler(
        IMeetingRepository meetingRepository,
        IMeetingParticipantRepository participantRepository,
        IUnitOfWork unitOfWork)
    {
        _meetingRepository = meetingRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantidan katilimci cikarma yetkiniz yok.");

        var participant = await _participantRepository.GetByIdAsync(request.ParticipantId)
            ?? throw new NotFoundException("Katilimci", request.ParticipantId);

        _participantRepository.Delete(participant);
        await _unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}
