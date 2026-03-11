using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Extensions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Queries;

public record GetMeetingsQuery(PaginationRequest Pagination) : IRequest<PaginatedResponse<MeetingListDto>>;

public class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, PaginatedResponse<MeetingListDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserProvider _currentUser;

    public GetMeetingsQueryHandler(IMeetingRepository meetingRepository, IMapper mapper, ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResponse<MeetingListDto>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var query = _meetingRepository.Query
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .Where(m => m.CreatedByUserId == userId
                     || m.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(m => m.StartDate);

        var paginated = await query.ToPaginatedResponseAsync(
            request.Pagination.PageNumber,
            request.Pagination.PageSize);

        return new PaginatedResponse<MeetingListDto>
        {
            Success = paginated.Success,
            Data = _mapper.Map<List<MeetingListDto>>(paginated.Data),
            PageNumber = paginated.PageNumber,
            PageSize = paginated.PageSize,
            TotalCount = paginated.TotalCount
        };
    }
}
