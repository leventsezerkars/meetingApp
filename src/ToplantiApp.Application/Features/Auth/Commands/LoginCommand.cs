using AutoMapper;
using FluentValidation;
using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Auth.Commands;

public record LoginCommand(LoginDto Data) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Data.Email).NotEmpty().EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Data.Password).NotEmpty().WithMessage("Sifre alani zorunludur.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService, IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Data.Email)
            ?? throw new AppException("E-posta veya sifre hatali.", 401);

        if (!BCrypt.Net.BCrypt.Verify(request.Data.Password, user.PasswordHash))
            throw new AppException("E-posta veya sifre hatali.", 401);

        var token = _tokenService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto(token, userDto);
    }
}
