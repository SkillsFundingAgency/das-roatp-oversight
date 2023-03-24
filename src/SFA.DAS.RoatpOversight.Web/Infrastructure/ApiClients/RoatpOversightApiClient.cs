using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class RoatpOversightApiClient : RoatpApiClientBase<RoatpOversightApiClient>, IRoatpOversightApiClient
    {
        public RoatpOversightApiClient(HttpClient httpClient, ILogger<RoatpOversightApiClient> logger, IRoatpRegisterTokenService tokenService)
            : base(httpClient.BaseAddress.ToString(), logger, tokenService)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_client.BaseAddress));
        }
       
        public async Task<bool> CreateProvider(CreateRoatpV2ProviderRequest providerRequest)
        {
            var result = await Post($"/providers", providerRequest);
            return await Task.FromResult(result == HttpStatusCode.OK);
        }
    }
}