using System.Text;
using JorgeCostaMacia.Http.Serilog.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Tests.Infrastructure;

public class EnrichRequestMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReadsBody_ThenRewindsSoTheEndpointCanReadIt()
    {
        bool nextCalled = false;
        EnrichRequestMiddleware middleware = new EnrichRequestMiddleware(context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        DefaultHttpContext httpContext = new DefaultHttpContext();
        byte[] payload = Encoding.UTF8.GetBytes("{\"name\":\"pepe\"}");
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.ContentLength = payload.Length;
        httpContext.Request.Body = new MemoryStream(payload);

        await middleware.InvokeAsync(httpContext);

        Assert.True(nextCalled);
        // the body was read for enrichment but rewound, so the endpoint still starts from the beginning.
        Assert.Equal(0, httpContext.Request.Body.Position);
    }
}
