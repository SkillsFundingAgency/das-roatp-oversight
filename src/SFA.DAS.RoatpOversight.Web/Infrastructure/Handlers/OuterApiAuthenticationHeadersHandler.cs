using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.Handlers;

[ExcludeFromCodeCoverage]
public class OuterApiAuthenticationHeadersHandler : DelegatingHandler
{
    private readonly string _apimSubscriptionKey;

    public OuterApiAuthenticationHeadersHandler(string apimSubscriptionKey)
    {
        _apimSubscriptionKey = apimSubscriptionKey;
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("accept", "application/json");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _apimSubscriptionKey);
        request.Headers.Add("X-Version", "1");

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
