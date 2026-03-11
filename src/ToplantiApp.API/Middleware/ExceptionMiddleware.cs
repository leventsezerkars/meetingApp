using System.Text.Json;
using FluentValidation;
using ToplantiApp.Application.Common.Exceptions;
using ToplantiApp.Application.Common.Models;

namespace ToplantiApp.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message),
            ValidationException valEx => (400, valEx.Message),
            _ => (500, "Beklenmeyen bir hata oluştu.")
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Beklenmeyen hata: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = Response.Fail(message, statusCode);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
