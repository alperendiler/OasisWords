using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;

namespace OasisWords.WebAPI.Controllers;

[EnableRateLimiting("auth")]
public class AuthController : BaseController
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        RegisterResponse response = await Mediator.Send(command, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        command.IpAddress = GetIpAddress();
        LoginResponse response = await Mediator.Send(command, cancellationToken);
        return Ok(response);
    }
}
