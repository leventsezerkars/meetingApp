using MediatR;
using Microsoft.AspNetCore.Mvc;
using ToplantiApp.Application.Common;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Application.Features.MeetingAccess.Queries;

namespace ToplantiApp.API.Controllers;

[ApiController]
[Route("api/meeting-room")]
public class MeetingAccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingAccessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{accessToken:guid}")]
    public async Task<ActionResult<Response<MeetingAccessResultDto>>> GetByAccessToken(Guid accessToken)
    {
        var result = await _mediator.Send(new GetMeetingByAccessTokenQuery(accessToken));

        if (result.Data != null && !result.Data.IsAccessible)
            return StatusCode(403, result);

        return Ok(result);
    }
}
