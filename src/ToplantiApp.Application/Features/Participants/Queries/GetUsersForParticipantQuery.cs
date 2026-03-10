using AutoMapper;
using MediatR;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Queries;

public record GetUsersForParticipantQuery(string SearchTerm) : IRequest<List<UserDto>>;

public class GetUsersForParticipantQueryHandler : IRequestHandler<GetUsersForParticipantQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersForParticipantQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersForParticipantQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchUsersAsync(request.SearchTerm);
        return _mapper.Map<List<UserDto>>(users);
    }
}
