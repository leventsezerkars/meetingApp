namespace ToplantiApp.Application.Common.Models;

public class Response
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; } = 200;

    public static Response Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static Response Fail(string message, int statusCode = 400)
        => new() { Success = false, Message = message, StatusCode = statusCode };
}

public class Response<T> : Response
{
    public T? Data { get; set; }

    public static Response<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public new static Response<T> Fail(string message, int statusCode = 400)
        => new() { Success = false, Message = message, StatusCode = statusCode };
}
