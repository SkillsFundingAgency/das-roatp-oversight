using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class RoatpRegisterApiClient : RoatpApiClientBase<RoatpRegisterApiClient>, IRoatpRegisterApiClient
    {
        public RoatpRegisterApiClient(HttpClient httpClient, ILogger<RoatpRegisterApiClient> logger, IRoatpRegisterTokenService tokenService) 
            : base(httpClient.BaseAddress.ToString(), logger, tokenService)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_client.BaseAddress));
        }
        public async Task<bool> CreateOrganisation(CreateRoatpOrganisationRequest organisationRequest)
        {
            var result = await Post($"/api/v1/organisation/create", organisationRequest);
            return await Task.FromResult(result == HttpStatusCode.OK);
        }

        public async Task<OrganisationRegisterStatus> GetOrganisationRegisterStatus(GetOrganisationRegisterStatusRequest request)
        {
            return await Get<OrganisationRegisterStatus>($"/api/v1/organisation/register-status?ukprn={request.UKPRN}");
        }

        public async Task<bool> UpdateApplicationDeterminedDate(UpdateOrganisationApplicationDeterminedDateRequest request)
        {
            var result = await Put($"/api/v1/updateorganisation/applicationDeterminedDate", request);
            return await Task.FromResult(result == HttpStatusCode.OK);
        }
    }
}
