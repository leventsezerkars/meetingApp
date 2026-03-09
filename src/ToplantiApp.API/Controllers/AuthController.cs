using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ToplantiApp.Application.DTOs;
using ToplantiApp.Application.Features.Auth.Commands;

namespace ToplantiApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromForm] RegisterDto data,
        IFormFile? profileImage)
    {
        Stream? imageStream = profileImage?.OpenReadStream();
        var command = new RegisterCommand(data, imageStream, profileImage?.FileName);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto data)
    {
        var result = await _mediator.Send(new LoginCommand(data));
        return Ok(result);
    }
}
