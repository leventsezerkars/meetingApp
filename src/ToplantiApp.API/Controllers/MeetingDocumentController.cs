using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Application.Features.Meetings.Commands;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.API.Controllers;

[ApiController]
[Route("api/meetings/{meetingId}/documents")]
[Authorize]
public class MeetingDocumentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileService _fileService;
    private readonly IMeetingRepository _meetingRepository;

    public MeetingDocumentController(IMediator mediator, IFileService fileService, IMeetingRepository meetingRepository)
    {
        _mediator = mediator;
        _fileService = fileService;
        _meetingRepository = meetingRepository;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<MeetingDocumentDto>> Upload(int meetingId, IFormFile file, [FromQuery] bool compress = true)
    {
        var command = new UploadMeetingDocumentCommand(
            meetingId, file.OpenReadStream(), file.FileName, file.ContentType, GetUserId(), compress);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{documentId}")]
    public async Task<IActionResult> Download(int meetingId, int documentId)
    {
        var meeting = await _meetingRepository.GetByIdWithDetailsAsync(meetingId);
        if (meeting == null) return NotFound();

        var document = meeting.Documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null) return NotFound();

        var (data, contentType, fileName) = await _fileService.GetFileAsync(document.FilePath, document.IsCompressed);
        return File(data, contentType, document.OriginalFileName);
    }
}
