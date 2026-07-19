using System.Text.Json;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JorgeCostaMacia.Http.ProblemDetails.Infrastructure;

/// <summary>
/// Extensions for <see cref="ProblemDetailsOptions"/> that apply our default RFC 7807 customization for
/// consistent error bodies. Kept as a <see cref="ProblemDetailsOptions"/> extension (not a hidden
/// <c>Add…</c> facade) so the <c>AddProblemDetails</c> call stays visible in the host's <c>Program</c>
/// while the policy lives here.
/// </summary>
public static class ProblemDetailsOptionsExtensions
{
    /// <summary>
    /// Sets <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> to a customizer that adds
    /// <c>RequestId</c>, <c>TraceId</c>, and <c>NodeId</c> to every error response, and delegates to
    /// <see cref="DomainExceptionHandler"/>, <see cref="BadHttpRequestExceptionHandler"/>, or
    /// <see cref="FluentValidationExceptionHandler"/> (for a raw <see cref="FluentValidation.ValidationException"/>)
    /// depending on the exception type. For any other exception, sets <c>AggregateId</c>,
    /// <c>AggregateCode</c>, and <c>AggregateType</c> to <see langword="null"/>, and <c>Errors</c> to
    /// <see langword="null"/> unless the response is already a
    /// <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/> (in which case its own <c>Errors</c>
    /// dictionary, populated by ASP.NET Core's model validation, is left untouched). All keys are converted
    /// through the application's configured <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ProblemDetailsOptions WithDefaults(this ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = Customize;

        return options;
    }

    private static void Customize(Microsoft.AspNetCore.Http.ProblemDetailsContext context)
    {
        JsonNamingPolicy namingPolicy = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions[namingPolicy.ConvertName("RequestId")] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions[namingPolicy.ConvertName("TraceId")] = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity?.TraceId.ToString();
        context.ProblemDetails.Extensions[namingPolicy.ConvertName("NodeId")] = Environment.MachineName;

        if (context.Exception is DomainException domainEx)
        {
            DomainExceptionHandler.Handle(context, domainEx, namingPolicy);
        }
        else if (context.Exception is BadHttpRequestException badHttpRequestEx)
        {
            BadHttpRequestExceptionHandler.Handle(context, badHttpRequestEx, namingPolicy);
        }
        else if (context.Exception is FluentValidation.ValidationException fluentValidationEx)
        {
            FluentValidationExceptionHandler.Handle(context, fluentValidationEx, namingPolicy);
        }
        else
        {
            context.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateId")] = null;
            context.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateCode")] = null;
            context.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateType")] = null;

            if (context.ProblemDetails is not Microsoft.AspNetCore.Mvc.ValidationProblemDetails)
            {
                context.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = null;
            }
        }
    }
}
