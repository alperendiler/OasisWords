using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OasisWords.Application.Features.Words.Commands.CreateWord;
using OasisWords.Application.Features.Words.Commands.CreateWordMeaning;
using OasisWords.Application.Features.Words.Queries.GetByCefrLevelWord;
using OasisWords.Application.Features.Words.Queries.GetListWord;
using OasisWords.Core.Application.Requests;
using OasisWords.Domain.Enums;

namespace OasisWords.WebAPI.Controllers;

[Authorize]
[EnableRateLimiting("global")]
public class WordsController : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(GetListWordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? languageId = null,
        CancellationToken cancellationToken = default)
    {
        GetListWordQuery query = new()
        {
            PageRequest = new PageRequest { PageIndex = pageIndex, PageSize = pageSize },
            LanguageId = languageId
        };
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    [HttpGet("by-cefr")]
    [ProducesResponseType(typeof(GetByCefrLevelWordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCefrLevel(
        [FromQuery] CefrLevel cefrLevel,
        [FromQuery] Guid languageId,
        [FromQuery] Guid translationLanguageId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        GetByCefrLevelWordQuery query = new()
        {
            CefrLevel = cefrLevel,
            LanguageId = languageId,
            TranslationLanguageId = translationLanguageId,
            PageRequest = new PageRequest { PageIndex = pageIndex, PageSize = pageSize }
        };
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateWordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWordCommand command,
        CancellationToken cancellationToken)
    {
        CreateWordResponse response = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetList), new { }, response);
    }

    [HttpPost("meanings")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateWordMeaningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMeaning(
        [FromBody] CreateWordMeaningCommand command,
        CancellationToken cancellationToken)
    {
        CreateWordMeaningResponse response = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetList), new { }, response);
    }
}
