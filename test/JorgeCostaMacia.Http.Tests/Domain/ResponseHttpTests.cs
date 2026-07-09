using JorgeCostaMacia.Http.Domain;

namespace JorgeCostaMacia.Http.Tests.Domain;

public class ResponseHttpTests
{
    private sealed record TestResponse(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : ResponseHttp(Id, Correlation, OccurredAt);

    [Fact]
    public void Defaults_generate_id_correlation_and_utc_timestamp()
    {
        var before = DateTime.UtcNow;
        var response = new TestResponse(null, null, null);

        Assert.NotEqual(Guid.Empty, response.AggregateId);
        Assert.Equal(response.AggregateId, response.AggregateCorrelationId);
        Assert.True(response.AggregateOccurredAt >= before);
        Assert.True(response.AggregateOccurredAt <= DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, response.AggregateOccurredAt.Kind);
    }

    [Fact]
    public void Correlation_defaults_to_the_supplied_id()
    {
        var id = Guid.NewGuid();

        var response = new TestResponse(id, null, null);

        Assert.Equal(id, response.AggregateCorrelationId);
    }

    [Fact]
    public void Keeps_supplied_values()
    {
        var id = Guid.NewGuid();
        var correlation = Guid.NewGuid();
        var occurredAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var response = new TestResponse(id, correlation, occurredAt);

        Assert.Equal(id, response.AggregateId);
        Assert.Equal(correlation, response.AggregateCorrelationId);
        Assert.Equal(occurredAt, response.AggregateOccurredAt);
    }
}
