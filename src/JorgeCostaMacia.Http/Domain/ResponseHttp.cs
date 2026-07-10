namespace JorgeCostaMacia.Http.Domain;

/// <summary>
/// Provides a base implementation for all HTTP Response messages within the domain.
/// Centralizes metadata required for message correlation, causality tracking, and auditing.
/// </summary>
/// <remarks>
/// <para>
/// This record serves as the standard payload wrapper for outgoing domain responses.
/// By embedding traceability identifiers, it allows clients and downstream systems
/// to correlate the response back to the original request or workflow.
/// </para>
///
/// <para>Properties include:</para>
/// <list type="bullet">
///    <item><description><see cref="AggregateId"/>: Unique identifier for the aggregate root or entity targeted by the operation.</description></item>
///    <item><description><see cref="AggregateCorrelationId"/>: Links this response to the specific distributed transaction context.</description></item>
///    <item><description><see cref="AggregateOccurredAt"/>: UTC timestamp indicating when the response was generated.</description></item>
/// </list>
///
/// <para>
/// As an <c>abstract record</c>, it guarantees immutability and value-based equality, ensuring
/// integrity during the serialization and transmission process across the HTTP layer. This base
/// type intentionally exposes only a <c>protected</c> constructor: it is not meant to be invoked
/// directly, but rather forwarded to by the public constructor of each concrete derived record
/// via <c>: base(...)</c>.
/// </para>
///
/// <para>
/// Note: the <c>Aggregate</c> prefix follows this package's internal naming convention,
/// consistent with the rest of the DDD-based ecosystem. It serves three purposes: linking
/// the same identifier across HTTP requests, domain events, and aggregates; avoiding name
/// collisions with properties declared by derived types; and signaling, by convention, that
/// a member relates to the aggregate rather than being response-specific.
/// </para>
/// </remarks>
public abstract record ResponseHttp : IResponseHttp
{
    /// <summary>
    /// Unique identifier of the aggregate root that this response targets.
    /// Crucial for ensuring the response is processed within the context of a specific domain entity.
    /// </summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    /// Correlation identifier used to group multiple messages (requests, commands, responses)
    /// related to the same aggregate workflow or distributed transaction.
    /// </summary>
    public Guid AggregateCorrelationId { get; init; }

    /// <summary>
    /// UTC timestamp representing exactly when this response instance was created.
    /// Used for causality tracking, auditing, and latency measurement purposes.
    /// </summary>
    public DateTime AggregateOccurredAt { get; init; }

    /// <summary>
    /// Initializes a new <see cref="ResponseHttp"/> instance with nullable traceability metadata,
    /// generating automatic defaults whenever a specific value is not supplied.
    /// </summary>
    /// <param name="aggregateId">
    /// The aggregate root identifier. If <c>null</c>, a new <see cref="Guid"/> will be generated
    /// by the GuidFactory (time-ordered UUIDv7 on .NET 9+, UUIDv4 on .NET 8).
    /// </param>
    /// <param name="aggregateCorrelationId">
    /// The correlation identifier. If <c>null</c>, it defaults to the value of <paramref name="aggregateId"/>.
    /// </param>
    /// <param name="aggregateOccurredAt">
    /// The UTC timestamp when the response was issued. If <c>null</c>, it defaults to <see cref="DateTime.UtcNow"/>.
    /// </param>
    protected ResponseHttp(Guid? aggregateId, Guid? aggregateCorrelationId, DateTime? aggregateOccurredAt)
    {
        AggregateId = aggregateId ?? JorgeCostaMacia.GuidFactory.Domain.GuidFactory.Create();
        AggregateCorrelationId = aggregateCorrelationId ?? AggregateId;
        AggregateOccurredAt = aggregateOccurredAt ?? DateTime.UtcNow;
    }
}
