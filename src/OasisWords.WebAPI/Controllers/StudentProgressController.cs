using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OasisWords.Application.Features.StudentProgress.Commands.UpdateStudentStreak;
using OasisWords.Application.Features.StudentProgress.Commands.UpdateWordProgress;
using OasisWords.Application.Features.StudentProgress.Queries.GetDailyTargetWords;
using OasisWords.Core.Security.Extensions;

namespace OasisWords.WebAPI.Controllers;

[Authorize]
[EnableRateLimiting("global")]
public class StudentProgressController : BaseController
{
    [HttpGet("daily-targets")]
    [ProducesResponseType(typeof(GetDailyTargetWordsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyTargets(
        [FromQuery] Guid studentId,
        CancellationToken cancellationToken)
    {
        GetDailyTargetWordsQuery query = new() { StudentId = studentId };
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    [HttpPost("progress")]
    [ProducesResponseType(typeof(UpdateWordProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProgress(
        [FromBody] UpdateWordProgressCommand command,
        CancellationToken cancellationToken)
    {
        UpdateWordProgressResponse response = await Mediator.Send(command, cancellationToken);

        // Also update streak whenever a word is answered
        await Mediator.Send(new UpdateStudentStreakCommand { StudentId = command.StudentId }, cancellationToken);

        return Ok(response);
    }

    [HttpPost("streak")]
    [ProducesResponseType(typeof(UpdateStudentStreakResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStreak(
        [FromBody] UpdateStudentStreakCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(command, cancellationToken));
    }
}
