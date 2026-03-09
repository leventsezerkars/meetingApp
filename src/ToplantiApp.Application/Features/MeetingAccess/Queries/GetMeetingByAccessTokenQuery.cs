using AutoMapper;
using MediatR;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.MeetingAccess.Queries;

public record GetMeetingByAccessTokenQuery(Guid AccessToken) : IRequest<MeetingAccessResultDto>;

public class GetMeetingByAccessTokenQueryHandler : IRequestHandler<GetMeetingByAccessTokenQuery, MeetingAccessResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMeetingByAccessTokenQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MeetingAccessResultDto> Handle(GetMeetingByAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByAccessTokenAsync(request.AccessToken);

        if (meeting == null)
            return new MeetingAccessResultDto(false, "Toplanti bulunamadi.", null);

        if (meeting.Status == MeetingStatus.Cancelled)
            return new MeetingAccessResultDto(false, "Bu toplanti iptal edilmistir.", null);

        var now = DateTime.UtcNow;

        if (now < meeting.StartDate)
            return new MeetingAccessResultDto(false,
                $"Toplanti henuz baslamadi. Baslangic: {meeting.StartDate:dd.MM.yyyy HH:mm}", null);

        if (now > meeting.EndDate)
            return new MeetingAccessResultDto(false, "Toplanti sona ermistir.", null);

        var meetingRoom = _mapper.Map<MeetingRoomDto>(meeting);
        return new MeetingAccessResultDto(true, null, meetingRoom);
    }
}
