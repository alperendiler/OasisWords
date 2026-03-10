using FluentValidation;
using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Commands.CreateWordMeaning;

public class CreateWordMeaningCommand : IRequest<CreateWordMeaningResponse>
{
    public Guid WordId { get; set; }
    public Guid TranslationLanguageId { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
}

public class CreateWordMeaningResponse
{
    public Guid Id { get; set; }
    public Guid WordId { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
}

public class CreateWordMeaningCommandHandler : IRequestHandler<CreateWordMeaningCommand, CreateWordMeaningResponse>
{
    private readonly IWordMeaningRepository _wordMeaningRepository;
    private readonly WordBusinessRules _wordBusinessRules;

    public CreateWordMeaningCommandHandler(
        IWordMeaningRepository wordMeaningRepository,
        WordBusinessRules wordBusinessRules)
    {
        _wordMeaningRepository = wordMeaningRepository;
        _wordBusinessRules = wordBusinessRules;
    }

    public async Task<CreateWordMeaningResponse> Handle(
        CreateWordMeaningCommand request,
        CancellationToken cancellationToken)
    {
        await _wordBusinessRules.WordShouldExist(request.WordId, cancellationToken);
        await _wordBusinessRules.LanguageShouldExist(request.TranslationLanguageId, cancellationToken);
        await _wordBusinessRules.WordMeaningCannotBeDuplicatedForSameLevelAndLanguage(
            request.WordId,
            request.TranslationLanguageId,
            request.CefrLevel,
            cancellationToken);

        WordMeaning meaning = new()
        {
            WordId = request.WordId,
            TranslationLanguageId = request.TranslationLanguageId,
            CefrLevel = request.CefrLevel,
            TranslationText = request.TranslationText,
            ExampleSentence = request.ExampleSentence,
            ExampleTranslation = request.ExampleTranslation
        };

        WordMeaning created = await _wordMeaningRepository.AddAsync(meaning, cancellationToken);

        return new CreateWordMeaningResponse
        {
            Id = created.Id,
            WordId = created.WordId,
            CefrLevel = created.CefrLevel,
            TranslationText = created.TranslationText
        };
    }
}

public class CreateWordMeaningCommandValidator : AbstractValidator<CreateWordMeaningCommand>
{
    public CreateWordMeaningCommandValidator()
    {
        RuleFor(x => x.WordId)
            .NotEmpty().WithMessage("Word is required.");

        RuleFor(x => x.TranslationLanguageId)
            .NotEmpty().WithMessage("Translation language is required.");

        RuleFor(x => x.CefrLevel)
            .IsInEnum().WithMessage("CEFR level must be a valid value (A1–C2).");

        RuleFor(x => x.TranslationText)
            .NotEmpty().WithMessage("Translation text is required.")
            .MaximumLength(500).WithMessage("Translation must not exceed 500 characters.");

        RuleFor(x => x.ExampleSentence)
            .MaximumLength(1000).WithMessage("Example sentence must not exceed 1000 characters.")
            .When(x => x.ExampleSentence is not null);

        RuleFor(x => x.ExampleTranslation)
            .MaximumLength(1000).WithMessage("Example translation must not exceed 1000 characters.")
            .When(x => x.ExampleTranslation is not null);
    }
}
