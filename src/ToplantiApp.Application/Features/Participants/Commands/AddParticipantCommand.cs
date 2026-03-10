using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Participants.Commands;

public record AddParticipantCommand(int MeetingId, AddParticipantDto Data, int UserId) : IRequest<Response<ParticipantDto>>;

public class AddParticipantCommandValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantCommandValidator()
    {
        RuleFor(x => x.Data)
            .Must(d => d.UserId.HasValue || (!string.IsNullOrEmpty(d.Email) && !string.IsNullOrEmpty(d.FullName)))
            .WithMessage("Dahili kullanici icin UserId, harici katilimci icin Email ve Ad alanlari zorunludur.");
    }
}

public class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand, Response<ParticipantDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMeetingParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMailService _mailService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AddParticipantCommandHandler(
        IMeetingRepository meetingRepository,
        IUserRepository userRepository,
        IMeetingParticipantRepository participantRepository,
        IUnitOfWork unitOfWork,
        IMailService mailService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _meetingRepository = meetingRepository;
        _userRepository = userRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
        _mailService = mailService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Response<ParticipantDto>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantiya katilimci ekleme yetkiniz yok.");

        MeetingParticipant participant;

        if (request.Data.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(request.Data.UserId.Value)
                ?? throw new NotFoundException("Kullanici", request.Data.UserId.Value);

            if (await _participantRepository.IsParticipantAsync(request.MeetingId, user.Email))
                throw new ConflictException("Bu kullanici zaten katilimci olarak eklenmis.");

            participant = new MeetingParticipant
            {
                MeetingId = request.MeetingId,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                ParticipantType = ParticipantType.Internal
            };
        }
        else
        {
            if (await _participantRepository.IsParticipantAsync(request.MeetingId, request.Data.Email!))
                throw new ConflictException("Bu e-posta adresi zaten katilimci olarak eklenmis.");

            participant = new MeetingParticipant
            {
                MeetingId = request.MeetingId,
                Email = request.Data.Email!,
                FullName = request.Data.FullName!,
                ParticipantType = ParticipantType.External
            };
        }

        await _participantRepository.AddAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:4200";
        var meetingUrl = $"{frontendUrl}/meeting-room/{meeting.AccessToken}";

        _ = _mailService.SendMeetingInvitationAsync(
            participant.Email, participant.FullName, meeting, meetingUrl);

        return Response<ParticipantDto>.Ok(_mapper.Map<ParticipantDto>(participant));
    }
}
