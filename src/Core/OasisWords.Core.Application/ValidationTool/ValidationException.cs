using FluentValidation.Results;

namespace OasisWords.Core.Application.ValidationTool;

public class ValidationException : Exception
{
    public IEnumerable<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .Select(g => new ValidationError(g.Key, g.ToArray()));
    }
}

public class ValidationError
{
    public string Property { get; }
    public string[] Messages { get; }

    public ValidationError(string property, string[] messages)
    {
        Property = property;
        Messages = messages;
    }
}
