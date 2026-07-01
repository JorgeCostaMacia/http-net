using System.Text.Json;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JorgeCostaMacia.Http.ProblemDetails;

/// <summary>
/// Configures standardized RFC 7807 Problem Details for consistent error responses.
/// </summary>
public static class ProblemDetailsContext
{
    /// <summary>
    /// Adds <c>RequestId</c>, <c>TraceId</c>, and <c>NodeId</c> to every error response, and
    /// delegates to <see cref="DomainExceptionHandler"/> or <see cref="BadHttpRequestExceptionHandler"/>
    /// depending on the exception type. For any other exception, sets <c>AggregateId</c>,
    /// <c>AggregateCode</c>, and <c>AggregateType</c> to <see langword="null"/>, and
    /// <c>Errors</c> to <see langword="null"/> unless the response is already a
    /// <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/> (in which case its own <c>Errors</c> dictionary,
    /// populated by ASP.NET Core's model validation, is left untouched). All keys are converted
    /// through the application's configured <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
    /// </summary>
    public static IServiceCollection AddProblemDetailsContext(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                JsonNamingPolicy namingPolicy = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

                ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
                ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("RequestId")] = ctx.HttpContext.TraceIdentifier;
                ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("TraceId")] = ctx.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity?.TraceId.ToString();
                ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("NodeId")] = Environment.MachineName;

                if (ctx.Exception is DomainException domainEx)
                {
                    DomainExceptionHandler.Handle(ctx, domainEx, namingPolicy);
                }
                else if (ctx.Exception is BadHttpRequestException badHttpRequestEx)
                {
                    BadHttpRequestExceptionHandler.Handle(ctx, badHttpRequestEx, namingPolicy);
                }
                else
                {
                    ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateId")] = null;
                    ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateCode")] = null;
                    ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateType")] = null;

                    if (ctx.ProblemDetails is not Microsoft.AspNetCore.Mvc.ValidationProblemDetails)
                    {
                        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = null;
                    }
                }
            };
        });

        return services;
    }
}
