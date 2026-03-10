using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.MeetingAccess.Queries;

public record GetMeetingByAccessTokenQuery(Guid AccessToken) : IRequest<Response<MeetingAccessResultDto>>;

public class GetMeetingByAccessTokenQueryHandler : IRequestHandler<GetMeetingByAccessTokenQuery, Response<MeetingAccessResultDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;

    public GetMeetingByAccessTokenQueryHandler(IMeetingRepository meetingRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
    }

    public async Task<Response<MeetingAccessResultDto>> Handle(GetMeetingByAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByAccessTokenAsync(request.AccessToken);

        if (meeting == null)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto(false, "Toplanti bulunamadi.", null));

        if (meeting.Status == MeetingStatus.Cancelled)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto(false, "Bu toplanti iptal edilmistir.", null));

        var now = DateTime.UtcNow;

        if (now < meeting.StartDate)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto(false,
                $"Toplanti henuz baslamadi. Baslangic: {meeting.StartDate:dd.MM.yyyy HH:mm}", null));

        if (now > meeting.EndDate)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto(false, "Toplanti sona ermistir.", null));

        var meetingRoom = _mapper.Map<MeetingRoomDto>(meeting);
        return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto(true, null, meetingRoom));
    }
}
