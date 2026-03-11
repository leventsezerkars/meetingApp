using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record DeleteMeetingCommand(int Id) : IRequest<Response>;

public class DeleteMeetingCommandHandler : IRequestHandler<DeleteMeetingCommand, Response>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUser;

    public DeleteMeetingCommandHandler(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Response> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var meeting = await _meetingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        if (meeting.CreatedByUserId != userId)
            throw new ForbiddenException("Bu toplantiyi silme yetkiniz yok.");

        if (meeting.StartDate <= DateTime.UtcNow)
            throw new AppException("Sadece baslamamis toplantilar silinebilir.", 400);

        _meetingRepository.Delete(meeting);
        await _unitOfWork.SaveChangesAsync();

        return Response.Ok("Toplanti silindi.");
    }
}
