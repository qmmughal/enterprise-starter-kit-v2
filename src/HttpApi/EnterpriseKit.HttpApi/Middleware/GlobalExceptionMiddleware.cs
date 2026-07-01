namespace EnterpriseKit.HttpApi.Middleware;

using EnterpriseKit.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Catches all unhandled exceptions and translates them into
/// RFC 7807 Problem Details responses.
///
/// Exception → HTTP Status mapping:
///   ValidationException    → 400 Bad Request
///   NotFoundException      → 404 Not Found
///   ForbiddenAccessException → 403 Forbidden
///   DomainException        → 422 Unprocessable Entity
///   Exception              → 500 Internal Server Error
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var problem = ex switch
        {
            ValidationException ve => BuildValidationProblem(ve, ctx),
            NotFoundException nfe  => BuildProblem(404, "Not Found", nfe.Message, nfe.Code, ctx),
            ForbiddenAccessException fa => BuildProblem(403, "Forbidden", fa.Message, fa.Code, ctx),
            DomainException de     => BuildProblem(422, "Domain Rule Violation", de.Message, de.Code, ctx),
            OperationCanceledException => BuildProblem(499, "Request Cancelled", "The request was cancelled.", "REQUEST_CANCELLED", ctx),
            _                      => BuildProblem(500, "Internal Server Error", GetSafeMessage(ex), "INTERNAL_ERROR", ctx)
        };

        // Log appropriately based on severity
        if (problem.Status >= 500)
            logger.LogError(ex, "Server error: {Title} — {Detail}", problem.Title, problem.Detail);
        else
            logger.LogWarning("Client error {Status}: {Title} — {Detail}", problem.Status, problem.Title, problem.Detail);

        ctx.Response.StatusCode = problem.Status ?? 500;
        ctx.Response.ContentType = "application/problem+json";

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
    }

    private static ProblemDetails BuildProblem(
        int status, string title, string detail, string code, HttpContext ctx)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = ctx.TraceIdentifier;
        return problem;
    }

    private static ProblemDetails BuildValidationProblem(ValidationException ve, HttpContext ctx)
    {
        var errors = ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problem = new ProblemDetails
        {
            Status = 400,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = ctx.Request.Path
        };
        problem.Extensions["code"] = "VALIDATION_FAILED";
        problem.Extensions["traceId"] = ctx.TraceIdentifier;
        problem.Extensions["errors"] = errors;
        return problem;
    }

    private string GetSafeMessage(Exception ex)
    {
        // Never expose raw exception details in production
        return env.IsDevelopment()
            ? $"{ex.GetType().Name}: {ex.Message}"
            : "An unexpected error occurred. Please contact support if the problem persists.";
    }
}
