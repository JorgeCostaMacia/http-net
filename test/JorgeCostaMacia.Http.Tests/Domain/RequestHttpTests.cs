using JorgeCostaMacia.Http.Domain;

namespace JorgeCostaMacia.Http.Tests.Domain;

public class RequestHttpTests
{
    private sealed record TestRequest(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : RequestHttp(Id, Correlation, OccurredAt);

    [Fact]
    public void Defaults_generate_id_correlation_and_utc_timestamp()
    {
        var before = DateTime.UtcNow;
        var request = new TestRequest(null, null, null);

        Assert.NotEqual(Guid.Empty, request.AggregateId);
        Assert.Equal(request.AggregateId, request.AggregateCorrelationId);
        Assert.True(request.AggregateOccurredAt >= before);
        Assert.True(request.AggregateOccurredAt <= DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, request.AggregateOccurredAt.Kind);
    }

    [Fact]
    public void Correlation_defaults_to_the_supplied_id()
    {
        var id = Guid.NewGuid();

        var request = new TestRequest(id, null, null);

        Assert.Equal(id, request.AggregateCorrelationId);
    }

    [Fact]
    public void Keeps_supplied_values()
    {
        var id = Guid.NewGuid();
        var correlation = Guid.NewGuid();
        var occurredAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var request = new TestRequest(id, correlation, occurredAt);

        Assert.Equal(id, request.AggregateId);
        Assert.Equal(correlation, request.AggregateCorrelationId);
        Assert.Equal(occurredAt, request.AggregateOccurredAt);
    }
}
