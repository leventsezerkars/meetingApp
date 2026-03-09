using AutoMapper;
using MediatR;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Queries;

public record GetMeetingsQuery(int UserId) : IRequest<List<MeetingListDto>>;

public class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, List<MeetingListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMeetingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<MeetingListDto>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        var meetings = await _unitOfWork.Meetings.GetByUserIdAsync(request.UserId);
        return _mapper.Map<List<MeetingListDto>>(meetings);
    }
}
