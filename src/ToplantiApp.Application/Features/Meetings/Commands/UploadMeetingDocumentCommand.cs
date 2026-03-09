using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record UploadMeetingDocumentCommand(
    int MeetingId,
    Stream FileStream,
    string FileName,
    string ContentType,
    int UserId,
    bool Compress = true) : IRequest<MeetingDocumentDto>;

public class UploadMeetingDocumentCommandHandler : IRequestHandler<UploadMeetingDocumentCommand, MeetingDocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public UploadMeetingDocumentCommandHandler(IUnitOfWork unitOfWork, IFileService fileService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<MeetingDocumentDto> Handle(UploadMeetingDocumentCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu toplantiya dokuman yukleme yetkiniz yok.");

        var (fileName, filePath, fileSize, isCompressed) = await _fileService.UploadFileAsync(
            request.FileStream, request.FileName, request.ContentType, $"meetings/{request.MeetingId}", request.Compress);

        var document = new MeetingDocument
        {
            MeetingId = request.MeetingId,
            FileName = fileName,
            OriginalFileName = request.FileName,
            FilePath = filePath,
            ContentType = request.ContentType,
            FileSize = fileSize,
            IsCompressed = isCompressed
        };

        await _unitOfWork.Meetings.AddAsync(meeting);
        meeting.Documents.Add(document);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MeetingDocumentDto>(document);
    }
}
