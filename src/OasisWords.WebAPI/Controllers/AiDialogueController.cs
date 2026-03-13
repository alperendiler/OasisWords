using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OasisWords.Application.Features.AiDialogue.Commands.SendMessage;
using OasisWords.Application.Features.AiDialogue.Commands.StartDialogueSession;
using OasisWords.Application.Features.AiDialogue.DTOs;
using OasisWords.Application.Features.AiDialogue.Queries.GetDialogueHistory;
using OasisWords.Core.Application.Requests;

namespace OasisWords.WebAPI.Controllers;

/// <summary>
/// AI conversation endpoints.
/// The entire controller is throttled to 2 req/s per the "ai_strict" policy
/// to protect Gemini API quota from abuse.
/// </summary>
[Authorize]
[EnableRateLimiting("ai_strict")]
public class AiDialogueController : BaseController
{
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(StartDialogueSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartSession(
        [FromBody] StartDialogueSessionCommand command,
        CancellationToken cancellationToken)
    {
        StartDialogueSessionResponse response = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSessionDetail), new { sessionId = response.SessionId }, response);
    }

    [HttpPost("sessions/{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendMessage(
        Guid sessionId,
        [FromBody] SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        command.SessionId = sessionId;
        return Ok(await Mediator.Send(command, cancellationToken));
    }

    // History reads don't hit Gemini — relax the rate limit for GET endpoints
    [EnableRateLimiting("global")]
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(GetDialogueHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid studentId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetDialogueHistoryQuery query = new()
        {
            StudentId = studentId,
            PageRequest = new PageRequest { PageIndex = pageIndex, PageSize = pageSize }
        };
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    [EnableRateLimiting("global")]
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(DialogueSessionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionDetail(
        Guid sessionId,
        [FromQuery] Guid studentId,
        CancellationToken cancellationToken)
    {
        GetDialogueSessionDetailQuery query = new()
        {
            SessionId = sessionId,
            StudentId = studentId
        };
        return Ok(await Mediator.Send(query, cancellationToken));
    }
}
