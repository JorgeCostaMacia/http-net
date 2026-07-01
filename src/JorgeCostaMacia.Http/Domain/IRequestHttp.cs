namespace JorgeCostaMacia.Http.Domain;

/// <summary>
/// Defines a marker interface for all HTTP request contracts within the domain.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as a foundational contract for all Data Transfer Objects (DTOs)
/// and command-like structures that enter the system via the HTTP transport layer.
/// It enables generic constraints and polymorphic handling of incoming web requests
/// throughout the middleware and application services.
/// </para>
/// </remarks>
public interface IRequestHttp { }
