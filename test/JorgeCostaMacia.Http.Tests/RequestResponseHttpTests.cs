using JorgeCostaMacia.Http.Domain;

namespace JorgeCostaMacia.Http.Tests;

public class RequestResponseHttpTests
{
    private sealed record TestRequest(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : RequestHttp(Id, Correlation, OccurredAt);

    private sealed record TestResponse(Guid? Id, Guid? Correlation, DateTime? OccurredAt)
        : ResponseHttp(Id, Correlation, OccurredAt);

    [Fact]
    public void Request_defaults_generate_id_correlation_and_utc_timestamp()
    {
        var before = DateTime.UtcNow;
        var request = new TestRequest(null, null, null);

        Assert.NotEqual(Guid.Empty, request.AggregateId);
        Assert.Equal(request.AggregateId, request.AggregateCorrelationId);
        Assert.True(request.AggregateOccurredAt >= before);
    }

    [Fact]
    public void Request_keeps_supplied_values()
    {
        var id = Guid.NewGuid();
        var correlation = Guid.NewGuid();
        var occurredAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var request = new TestRequest(id, correlation, occurredAt);

        Assert.Equal(id, request.AggregateId);
        Assert.Equal(correlation, request.AggregateCorrelationId);
        Assert.Equal(occurredAt, request.AggregateOccurredAt);
    }

    [Fact]
    public void Response_defaults_generate_id_correlation_and_utc_timestamp()
    {
        var before = DateTime.UtcNow;
        var response = new TestResponse(null, null, null);

        Assert.NotEqual(Guid.Empty, response.AggregateId);
        Assert.Equal(response.AggregateId, response.AggregateCorrelationId);
        Assert.True(response.AggregateOccurredAt >= before);
    }

    [Fact]
    public void Response_keeps_supplied_values()
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
