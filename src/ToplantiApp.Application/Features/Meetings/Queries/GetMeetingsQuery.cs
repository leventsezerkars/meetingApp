using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Queries;

public record GetMeetingsQuery(int UserId, PaginationRequest Pagination) : IRequest<PaginatedResponse<MeetingListDto>>;

public class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, PaginatedResponse<MeetingListDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;

    public GetMeetingsQueryHandler(IMeetingRepository meetingRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<MeetingListDto>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        var query = _meetingRepository.Query
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .Where(m => m.CreatedByUserId == request.UserId
                     || m.Participants.Any(p => p.UserId == request.UserId))
            .OrderByDescending(m => m.StartDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var meetings = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<MeetingListDto>
        {
            Success = true,
            Data = _mapper.Map<List<MeetingListDto>>(meetings),
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }
}
