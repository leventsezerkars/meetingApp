using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Commands;

public record RemoveParticipantCommand(int MeetingId, int ParticipantId) : IRequest<Response>;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Response>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUser;

    public RemoveParticipantCommandHandler(
        IMeetingRepository meetingRepository,
        IMeetingParticipantRepository participantRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Response> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != userId)
            throw new ForbiddenException("Bu toplantidan katilimci cikarma yetkiniz yok.");

        var participant = await _participantRepository.GetByIdAsync(request.ParticipantId)
            ?? throw new NotFoundException("Katilimci", request.ParticipantId);

        _participantRepository.Delete(participant);
        await _unitOfWork.SaveChangesAsync();

        return Response.Ok("Katilimci cikarildi.");
    }
}
