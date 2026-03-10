using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Application.Features.Meetings.Commands;
using ToplantiApp.Application.Features.Meetings.Queries;

namespace ToplantiApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MeetingListDto>>> GetAll([FromQuery] PaginationRequest pagination)
    {
        var result = await _mediator.Send(new GetMeetingsQuery(GetUserId(), pagination));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Response<MeetingDto>>> GetById(int id)
    {
        var result = await _mediator.Send(new GetMeetingByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<MeetingDto>>> Create([FromBody] CreateMeetingDto data)
    {
        var result = await _mediator.Send(new CreateMeetingCommand(data, GetUserId()));
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Response<MeetingDto>>> Update(int id, [FromBody] UpdateMeetingDto data)
    {
        var result = await _mediator.Send(new UpdateMeetingCommand(id, data, GetUserId()));
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<Response>> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelMeetingCommand(id, GetUserId()));
        return Ok(result);
    }
}
