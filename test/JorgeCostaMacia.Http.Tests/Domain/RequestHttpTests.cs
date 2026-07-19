using JorgeCostaMacia.Http.Domain;

namespace JorgeCostaMacia.Http.Tests.Domain;

public class RequestHttpTests
{
    private sealed record TestRequest(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : RequestHttp(Id, Correlation, OccurredAt);

    [Fact]
    public void Defaults_GenerateIdCorrelationAndUtcTimestamp()
    {
        DateTime before = DateTime.UtcNow;
        TestRequest request = new TestRequest(null, null, null);

        Assert.NotEqual(Guid.Empty, request.AggregateId);
        Assert.Equal(request.AggregateId, request.AggregateCorrelationId);
        Assert.True(request.AggregateOccurredAt >= before);
        Assert.True(request.AggregateOccurredAt <= DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, request.AggregateOccurredAt.Kind);
    }

    [Fact]
    public void Correlation_DefaultsToTheSuppliedId()
    {
        Guid id = Guid.NewGuid();

        TestRequest request = new TestRequest(id, null, null);

        Assert.Equal(id, request.AggregateCorrelationId);
    }

    [Fact]
    public void Keeps_SuppliedValues()
    {
        Guid id = Guid.NewGuid();
        Guid correlation = Guid.NewGuid();
        DateTime occurredAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        TestRequest request = new TestRequest(id, correlation, occurredAt);

        Assert.Equal(id, request.AggregateId);
        Assert.Equal(correlation, request.AggregateCorrelationId);
        Assert.Equal(occurredAt, request.AggregateOccurredAt);
    }
}
