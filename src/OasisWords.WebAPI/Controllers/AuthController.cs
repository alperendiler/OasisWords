using Microsoft.AspNetCore.Mvc;
using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;

namespace OasisWords.WebAPI.Controllers;

public class AuthController : BaseController
{
    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        RegisterResponse response = await Mediator.Send(command, cancellationToken);
        return Created(string.Empty, response);
    }

    /// <summary>Login with email and password. Returns JWT access token and refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        command.IpAddress = GetIpAddress();
        LoginResponse response = await Mediator.Send(command, cancellationToken);
        return Ok(response);
    }
}
