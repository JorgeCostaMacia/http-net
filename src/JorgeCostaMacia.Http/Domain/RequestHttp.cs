namespace JorgeCostaMacia.Http.Domain;

/// <summary>
/// Provides a base implementation for all HTTP Request messages within the domain.
/// Centralizes mandatory tracing, correlation, and auditing metadata required for distributed systems.
/// </summary>
/// <remarks>
/// <para>
/// This abstract record ensures that every incoming HTTP request is enriched with
/// traceability identifiers, fulfilling the requirements for correlation and causality
/// tracking across microservices and asynchronous workflows.
/// </para>
///
/// <para>Properties include:</para>
/// <list type="bullet">
///    <item><description><see cref="AggregateId"/>: Unique identifier for the specific request instance, used for idempotency.</description></item>
///    <item><description><see cref="AggregateCorrelationId"/>: Links related messages within a single distributed transaction or workflow.</description></item>
///    <item><description><see cref="AggregateOccurredAt"/>: UTC timestamp of when the request was generated.</description></item>
/// </list>
///
/// <para>
/// As an <c>abstract record</c>, it guarantees immutability and value-based equality.
/// This base type intentionally exposes only a <c>protected</c> constructor: it is not
/// meant to be invoked directly, but rather forwarded to by the public constructor of
/// each concrete derived record via <c>: base(...)</c>. This centralizes traceability
/// metadata initialization while letting derived types control their own public
/// construction surface (and, by extension, how they integrate with serialization
/// frameworks such as <c>System.Text.Json</c>).
/// </para>
///
/// <para>
/// Note: the <c>Aggregate</c> prefix follows this package's internal naming convention,
/// consistent with the rest of the DDD-based ecosystem. It serves three purposes: linking
/// the same identifier across HTTP requests, domain events, and aggregates (e.g. the
/// <see cref="AggregateId"/> used here is the same one later used to raise or operate on
/// the corresponding domain event/aggregate); avoiding name collisions with properties
/// declared by derived types; and signaling, by convention, that a member relates to the
/// aggregate rather than being request-specific.
/// </para>
/// </remarks>
public abstract record RequestHttp : IRequestHttp
{
    /// <summary>
    /// Unique identifier for this specific request instance.
    /// Crucial for idempotent processing and tracking the request's lifecycle in logs.
    /// </summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    /// Identifier used to correlate this request with other related operations,
    /// commands, or events in a distributed workflow context.
    /// </summary>
    public Guid AggregateCorrelationId { get; init; }

    /// <summary>
    /// UTC timestamp representing exactly when this request was issued.
    /// Used for causality tracking, ordering, and auditing purposes.
    /// </summary>
    public DateTime AggregateOccurredAt { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHttp"/> record with
    /// nullable traceability metadata, generating automatic defaults whenever
    /// a specific value is not supplied.
    /// </summary>
    /// <param name="aggregateId">
    /// The unique request identifier. If <c>null</c>, a new <see cref="Guid"/> will be generated
    /// using a time-ordered UUIDv7 factory.
    /// </param>
    /// <param name="aggregateCorrelationId">
    /// The correlation identifier. If <c>null</c>, it defaults to the value of <paramref name="aggregateId"/>.
    /// </param>
    /// <param name="aggregateOccurredAt">
    /// The UTC timestamp when the request was issued. If <c>null</c>, it defaults to <see cref="DateTime.UtcNow"/>.
    /// </param>
    protected RequestHttp(Guid? aggregateId, Guid? aggregateCorrelationId, DateTime? aggregateOccurredAt)
    {
        AggregateId = aggregateId ?? JorgeCostaMacia.GuidFactory.Domain.GuidFactory.Create();
        AggregateCorrelationId = aggregateCorrelationId ?? AggregateId;
        AggregateOccurredAt = aggregateOccurredAt ?? DateTime.UtcNow;
    }
}
