using System.Net;
using Microsoft.AspNetCore.Builder;
using ForwardedHeaderFlags = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;
using IPNetwork = System.Net.IPNetwork;

namespace JorgeCostaMacia.Http.ForwardedHeaders.Infrastructure;

/// <summary>
/// Extensions for <see cref="ForwardedHeadersOptions"/> that apply the default forwarded-headers policy of a
/// service published behind a reverse proxy: on the same host, or reaching the container through Docker's
/// default bridge. Kept as a <see cref="ForwardedHeadersOptions"/> extension (not a hidden <c>Add…</c> facade)
/// so the <c>Configure&lt;ForwardedHeadersOptions&gt;</c> call — and the <c>UseForwardedHeaders</c> that
/// applies it — stay visible in the host's <c>Program</c> while the policy lives here.
/// </summary>
public static class ForwardedHeadersOptionsExtensions
{
    /// <summary>Docker's default bridge network (<c>docker0</c>), the address a proxy on the host reaches a published container from.</summary>
    private static readonly IPNetwork _dockerBridgeNetwork = new IPNetwork(IPAddress.Parse("172.17.0.0"), 16);

    /// <summary>
    /// Applies the default forwarded-headers policy: the proxy's <c>X-Forwarded-For</c>, <c>X-Forwarded-Proto</c>
    /// and <c>X-Forwarded-Host</c> are honoured, and only when the request comes from a trusted address — the
    /// loopbacks (<c>127.0.0.1</c>, <c>127.0.1.1</c>, <c>::1</c>) or Docker's default bridge
    /// (<c>172.17.0.0/16</c>). Anything else is ignored, so a client cannot forge its address or scheme by
    /// sending the headers itself.
    /// </summary>
    /// <remarks>
    /// The trusted addresses are added to the framework's own (it already trusts the loopback), not put in
    /// their place: a service with another proxy adds it to the same options after this call.
    /// </remarks>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ForwardedHeadersOptions WithDefaults(this ForwardedHeadersOptions options)
    {
        options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
        options.KnownProxies.Add(IPAddress.Parse("127.0.1.1"));
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
        options.KnownIPNetworks.Add(_dockerBridgeNetwork);

        options.ForwardedHeaders = ForwardedHeaderFlags.XForwardedHost | ForwardedHeaderFlags.XForwardedFor | ForwardedHeaderFlags.XForwardedProto;

        return options;
    }
}
