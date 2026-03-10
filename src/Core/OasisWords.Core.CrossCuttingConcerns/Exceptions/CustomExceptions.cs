namespace OasisWords.Core.CrossCuttingConcerns.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class AuthorizationException : Exception
{
    public AuthorizationException(string message = "You are not authorized to perform this action.")
        : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
