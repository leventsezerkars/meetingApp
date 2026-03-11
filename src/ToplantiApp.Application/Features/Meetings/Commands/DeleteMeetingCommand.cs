using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record DeleteMeetingCommand(int Id, int UserId) : IRequest<Response>;

public class DeleteMeetingCommandHandler : IRequestHandler<DeleteMeetingCommand, Response>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMeetingCommandHandler(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Response> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantiyi silme yetkiniz yok.");

        if (meeting.StartDate <= DateTime.UtcNow)
            throw new AppException("Sadece baslamamis toplantilar silinebilir.", 400);

        _meetingRepository.Delete(meeting);
        await _unitOfWork.SaveChangesAsync();

        return Response.Ok("Toplanti silindi.");
    }
}
