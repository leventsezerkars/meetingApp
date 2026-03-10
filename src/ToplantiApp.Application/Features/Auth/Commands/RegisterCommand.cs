using AutoMapper;
using FluentValidation;
using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterDto Data, Stream? ProfileImage, string? ProfileImageFileName) : IRequest<AuthResponseDto>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Data.FirstName).NotEmpty().MaximumLength(100).WithMessage("Ad alani zorunludur.");
        RuleFor(x => x.Data.LastName).NotEmpty().MaximumLength(100).WithMessage("Soyad alani zorunludur.");
        RuleFor(x => x.Data.Email).NotEmpty().EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Data.Phone).NotEmpty().MaximumLength(20).WithMessage("Telefon alani zorunludur.");
        RuleFor(x => x.Data.Password).NotEmpty().MinimumLength(6).WithMessage("Sifre en az 6 karakter olmalidir.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMailService _mailService;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IMailService mailService,
        IFileService fileService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mailService = mailService;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Data.Email))
            throw new ConflictException("Bu e-posta adresi zaten kayitli.");

        var user = new User
        {
            FirstName = request.Data.FirstName,
            LastName = request.Data.LastName,
            Email = request.Data.Email,
            Phone = request.Data.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Data.Password)
        };

        if (request.ProfileImage != null && request.ProfileImageFileName != null)
        {
            var (_, filePath, _, _) = await _fileService.UploadFileAsync(
                request.ProfileImage, request.ProfileImageFileName, "image/*", "profiles");
            user.ProfileImagePath = filePath;
        }

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _ = _mailService.SendWelcomeEmailAsync(user);

        var token = _tokenService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto(token, userDto);
    }
}
