using JorgeCostaMacia.Http.Domain;

namespace JorgeCostaMacia.Http.Tests.Domain;

public class ResponseHttpTests
{
    private sealed record TestResponse(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : ResponseHttp(Id, Correlation, OccurredAt);

    [Fact]
    public void Defaults_GenerateIdCorrelationAndUtcTimestamp()
    {
        DateTime before = DateTime.UtcNow;
        TestResponse response = new TestResponse(null, null, null);

        Assert.NotEqual(Guid.Empty, response.AggregateId);
        Assert.Equal(response.AggregateId, response.AggregateCorrelationId);
        Assert.True(response.AggregateOccurredAt >= before);
        Assert.True(response.AggregateOccurredAt <= DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, response.AggregateOccurredAt.Kind);
    }

    [Fact]
    public void Correlation_DefaultsToTheSuppliedId()
    {
        Guid id = Guid.NewGuid();

        TestResponse response = new TestResponse(id, null, null);

        Assert.Equal(id, response.AggregateCorrelationId);
    }

    [Fact]
    public void Keeps_SuppliedValues()
    {
        Guid id = Guid.NewGuid();
        Guid correlation = Guid.NewGuid();
        DateTime occurredAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        TestResponse response = new TestResponse(id, correlation, occurredAt);

        Assert.Equal(id, response.AggregateId);
        Assert.Equal(correlation, response.AggregateCorrelationId);
        Assert.Equal(occurredAt, response.AggregateOccurredAt);
    }
}
