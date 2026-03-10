using FluentValidation;
using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.CreateWord;

public class CreateWordCommand : IRequest<CreateWordResponse>
{
    public Guid LanguageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }
}

public class CreateWordResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid LanguageId { get; set; }
}

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, CreateWordResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly WordBusinessRules _wordBusinessRules;

    public CreateWordCommandHandler(IWordRepository wordRepository, WordBusinessRules wordBusinessRules)
    {
        _wordRepository = wordRepository;
        _wordBusinessRules = wordBusinessRules;
    }

    public async Task<CreateWordResponse> Handle(CreateWordCommand request, CancellationToken cancellationToken)
    {
        await _wordBusinessRules.LanguageShouldExist(request.LanguageId, cancellationToken);
        await _wordBusinessRules.WordCannotBeDuplicatedForSameLanguage(request.LanguageId, request.Text, cancellationToken);

        Word word = new()
        {
            LanguageId = request.LanguageId,
            Text = request.Text.Trim().ToLower(),
            PhoneticSpelling = request.PhoneticSpelling,
            PronunciationAudioUrl = request.PronunciationAudioUrl
        };

        Word created = await _wordRepository.AddAsync(word, cancellationToken);

        return new CreateWordResponse
        {
            Id = created.Id,
            Text = created.Text,
            LanguageId = created.LanguageId
        };
    }
}

public class CreateWordCommandValidator : AbstractValidator<CreateWordCommand>
{
    public CreateWordCommandValidator()
    {
        RuleFor(x => x.LanguageId)
            .NotEmpty().WithMessage("Language is required.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Word text is required.")
            .MaximumLength(200).WithMessage("Word text must not exceed 200 characters.");

        RuleFor(x => x.PronunciationAudioUrl)
            .MaximumLength(512).WithMessage("Audio URL must not exceed 512 characters.")
            .When(x => x.PronunciationAudioUrl is not null);
    }
}
