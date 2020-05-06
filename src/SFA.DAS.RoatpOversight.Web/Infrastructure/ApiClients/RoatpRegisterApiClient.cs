using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using SFA.DAS.RoatpOversight.Web.Settings;
using System;
using System.Collections.Generic;
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken());
        }
        public async Task<bool> CreateOrganisation(CreateRoatpOrganisationRequest organisationRequest)
        {
            HttpStatusCode result = await Post<CreateRoatpOrganisationRequest>($"/api/v1/organisation/create", organisationRequest);

            return await Task.FromResult(result == HttpStatusCode.OK);
        }

    }
}
