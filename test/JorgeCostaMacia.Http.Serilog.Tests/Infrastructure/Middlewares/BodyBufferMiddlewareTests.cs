using JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Tests.Infrastructure.Middlewares;

public class BodyBufferMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_EnablesBuffering_ThenCallsNext()
    {
        bool nextCalled = false;
        BodyBufferMiddleware middleware = new BodyBufferMiddleware(context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        DefaultHttpContext httpContext = new DefaultHttpContext();

        await middleware.InvokeAsync(httpContext);

        Assert.True(httpContext.Request.Body.CanSeek);
        Assert.True(nextCalled);
    }
}
