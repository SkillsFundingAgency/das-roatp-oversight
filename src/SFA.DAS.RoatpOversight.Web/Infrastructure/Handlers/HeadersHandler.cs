using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using SFA.DAS.RoatpOversight.Domain.Interfaces;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.Handlers
{
    public class HeadersHandler: DelegatingHandler
    {
        private readonly RoatpOversightOuterApi _config;

        public HeadersHandler(IRoatpOversightOuterApi config)
        {
            _config = (RoatpOversightOuterApi)config;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
            request.Headers.Add("X-Version", "1");
        
           return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
