using AutoMapper;
using MediatR;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Queries;

public record GetUsersForParticipantQuery(string SearchTerm) : IRequest<List<UserDto>>;

public class GetUsersForParticipantQueryHandler : IRequestHandler<GetUsersForParticipantQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersForParticipantQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersForParticipantQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.SearchUsersAsync(request.SearchTerm);
        return _mapper.Map<List<UserDto>>(users);
    }
}
