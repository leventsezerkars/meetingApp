using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Queries;

public record GetMeetingByIdQuery(int Id) : IRequest<MeetingDto>;

public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, MeetingDto>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;

    public GetMeetingByIdQueryHandler(IMeetingRepository meetingRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
    }

    public async Task<MeetingDto> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        return _mapper.Map<MeetingDto>(meeting);
    }
}
