namespace ToplantiApp.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} bulunamadi. (Id: {key})", 404) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
