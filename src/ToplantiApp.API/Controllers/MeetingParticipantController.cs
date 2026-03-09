using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Application.Features.Participants.Commands;
using ToplantiApp.Application.Features.Participants.Queries;

namespace ToplantiApp.API.Controllers;

[ApiController]
[Route("api/meetings/{meetingId}/participants")]
[Authorize]
public class MeetingParticipantController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingParticipantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ParticipantDto>> Add(int meetingId, [FromBody] AddParticipantDto data)
    {
        var result = await _mediator.Send(new AddParticipantCommand(meetingId, data, GetUserId()));
        return Ok(result);
    }

    [HttpDelete("{participantId}")]
    public async Task<IActionResult> Remove(int meetingId, int participantId)
    {
        await _mediator.Send(new RemoveParticipantCommand(meetingId, participantId, GetUserId()));
        return NoContent();
    }

    [HttpGet("search-users")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string term)
    {
        var result = await _mediator.Send(new GetUsersForParticipantQuery(term));
        return Ok(result);
    }
}
