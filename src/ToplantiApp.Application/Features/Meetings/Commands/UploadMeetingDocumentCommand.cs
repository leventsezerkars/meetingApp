using AutoMapper;
using MediatR;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Application.Features.Meetings.Commands;

public record UploadMeetingDocumentCommand(
    int MeetingId,
    Stream FileStream,
    string FileName,
    string ContentType,
    bool Compress = true) : IRequest<Response<MeetingDocumentDto>>;

public class UploadMeetingDocumentCommandHandler : IRequestHandler<UploadMeetingDocumentCommand, Response<MeetingDocumentDto>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserProvider _currentUser;

    public UploadMeetingDocumentCommandHandler(
        IMeetingRepository meetingRepository,
        IUnitOfWork unitOfWork,
        IFileService fileService,
        IMapper mapper,
        ICurrentUserProvider currentUser)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Response<MeetingDocumentDto>> Handle(UploadMeetingDocumentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Kullanici kimligi alinamadi.");
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId)
            ?? throw new NotFoundException("Toplanti", request.MeetingId);

        if (meeting.CreatedByUserId != userId)
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

        meeting.Documents.Add(document);
        await _unitOfWork.SaveChangesAsync();

        return Response<MeetingDocumentDto>.Ok(_mapper.Map<MeetingDocumentDto>(document));
    }
}
