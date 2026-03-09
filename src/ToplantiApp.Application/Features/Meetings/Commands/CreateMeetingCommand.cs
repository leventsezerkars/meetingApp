using AutoMapper;
using FluentValidation;
using MediatR;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record CreateMeetingCommand(CreateMeetingDto Data, int UserId) : IRequest<MeetingDto>;

public class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(250).WithMessage("Toplanti adi zorunludur.");
        RuleFor(x => x.Data.StartDate).GreaterThan(DateTime.UtcNow).WithMessage("Baslangic tarihi gelecekte olmalidir.");
        RuleFor(x => x.Data.EndDate).GreaterThan(x => x.Data.StartDate).WithMessage("Bitis tarihi baslangictan sonra olmalidir.");
    }
}

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, MeetingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMeetingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MeetingDto> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = new Meeting
        {
            Name = request.Data.Name,
            Description = request.Data.Description,
            StartDate = request.Data.StartDate,
            EndDate = request.Data.EndDate,
            CreatedByUserId = request.UserId,
            AccessToken = Guid.NewGuid()
        };

        await _unitOfWork.Meetings.AddAsync(meeting);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meeting.Id);
        return _mapper.Map<MeetingDto>(created);
    }
}
