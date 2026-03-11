using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Queries;

public record GetMeetingByIdQuery(int Id) : IRequest<Response<MeetingDto>>;

public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, Response<MeetingDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;

    public GetMeetingByIdQueryHandler(IMeetingRepository meetingRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
    }

    public async Task<Response<MeetingDto>> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(request.Id)
            ?? throw new NotFoundException("Toplanti", request.Id);

        return Response<MeetingDto>.Ok(_mapper.Map<MeetingDto>(meeting));
    }
}
