using JorgeCostaMacia.Http.Serilog.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Tests.Infrastructure;

public class EnrichAuthenticationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNext_WithinTheLogContextScope()
    {
        bool nextCalled = false;
        EnrichAuthenticationMiddleware middleware = new EnrichAuthenticationMiddleware(context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(nextCalled);
    }
}
