using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Queries;

public record GetUsersForParticipantQuery(string SearchTerm) : IRequest<Response<List<UserDto>>>;

public class GetUsersForParticipantQueryHandler : IRequestHandler<GetUsersForParticipantQuery, Response<List<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersForParticipantQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Response<List<UserDto>>> Handle(GetUsersForParticipantQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchUsersAsync(request.SearchTerm);
        return Response<List<UserDto>>.Ok(_mapper.Map<List<UserDto>>(users));
    }
}
