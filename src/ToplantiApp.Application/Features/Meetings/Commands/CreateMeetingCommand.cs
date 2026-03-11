using AutoMapper;
using FluentValidation;
using MediatR;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record CreateMeetingCommand(CreateMeetingDto Data) : IRequest<Response<MeetingDto>>;

public class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(250).WithMessage("Toplanti adi zorunludur.");
        RuleFor(x => x.Data.StartDate).GreaterThan(DateTime.UtcNow).WithMessage("Baslangic tarihi gelecekte olmalidir.");
        RuleFor(x => x.Data.EndDate).GreaterThan(x => x.Data.StartDate).WithMessage("Bitis tarihi baslangictan sonra olmalidir.");
    }
}

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, Response<MeetingDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMeetingCommandHandler(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Response<MeetingDto>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = new Meeting
        {
            Name = request.Data.Name,
            Description = request.Data.Description,
            StartDate = request.Data.StartDate,
            EndDate = request.Data.EndDate,
            AccessToken = Guid.NewGuid()
        };

        await _meetingRepository.AddAsync(meeting);
        await _unitOfWork.SaveChangesAsync();

        var created = await _meetingRepository.GetByIdWithDetailsAsync(meeting.Id);
        return Response<MeetingDto>.Ok(_mapper.Map<MeetingDto>(created));
    }
}
