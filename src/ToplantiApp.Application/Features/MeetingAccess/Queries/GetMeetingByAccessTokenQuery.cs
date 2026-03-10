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
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto { IsAccessible = false, Message = "Toplantı bulunamadı." });

        if (meeting.Status == MeetingStatus.Cancelled)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto { IsAccessible = false, Message = "Bu toplantı iptal edilmiştir." });

        var now = DateTime.UtcNow;

        if (now < meeting.StartDate)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto
            {
                IsAccessible = false,
                Message = $"Toplantı henüz başlamadı. Başlangıç: {meeting.StartDate:dd.MM.yyyy HH:mm}"
            });

        if (now > meeting.EndDate)
            return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto { IsAccessible = false, Message = "Toplantı sona ermiştir." });

        var meetingRoom = _mapper.Map<MeetingRoomDto>(meeting);
        return Response<MeetingAccessResultDto>.Ok(new MeetingAccessResultDto { IsAccessible = true, Meeting = meetingRoom });
    }
}
