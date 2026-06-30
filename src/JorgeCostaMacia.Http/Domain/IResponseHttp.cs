namespace JorgeCostaMacia.Http.Domain;

/// <summary>
/// Defines a marker interface for all HTTP response contracts within the domain.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as a foundational contract for all Data Transfer Objects (DTOs)
/// and state representations sent back to the client via the HTTP transport layer.
/// It enables uniform handling of outgoing web responses and facilitates generic
/// constraints within response-handling infrastructure.
/// </para>
/// </remarks>
public interface IResponseHttp { }
