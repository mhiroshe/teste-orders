using FluentValidation;
using Microsoft.AspNetCore.Http;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace OrderManagement.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            NotFoundException nfe =>
                (HttpStatusCode.NotFound, "Not Found", nfe.Message, (object?)null),

            BadHttpRequestException bhre =>
                (HttpStatusCode.BadRequest, "Malformed Request", bhre.Message, (object?)null),

            DomainException de =>
                (HttpStatusCode.UnprocessableEntity, "Business Rule Violation", de.Message, (object?)null),

            ValidationException ve =>
                (HttpStatusCode.BadRequest, "Validation Failed", "One or more validation errors occurred.",
                 (object?)ve.Errors
                     .GroupBy(e => e.PropertyName)
                     .ToDictionary(
                         g => g.Key,
                         g => g.Select(e => e.ErrorMessage).ToArray()
                     )),

            _ => (HttpStatusCode.InternalServerError, "Internal Server Error",
                  "An unexpected error occurred.", (object?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
