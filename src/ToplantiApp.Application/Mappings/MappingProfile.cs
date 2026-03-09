using AutoMapper;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Meeting, MeetingListDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ParticipantCount, opt => opt.MapFrom(s => s.Participants.Count))
            .ForMember(d => d.CreatedByName, opt => opt.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : ""));

        CreateMap<Meeting, MeetingDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.MeetingUrl, opt => opt.Ignore());

        CreateMap<Meeting, MeetingRoomDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CreatedByName, opt => opt.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : ""));

        CreateMap<MeetingParticipant, ParticipantDto>()
            .ForMember(d => d.ParticipantType, opt => opt.MapFrom(s => s.ParticipantType.ToString()));

        CreateMap<MeetingDocument, MeetingDocumentDto>();
    }
}
