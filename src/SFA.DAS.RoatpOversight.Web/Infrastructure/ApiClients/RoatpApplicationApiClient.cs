using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class RoatpApplicationApiClient : ApiClientBase<RoatpApplicationApiClient>, IRoatpApplicationApiClient
    {
        public RoatpApplicationApiClient(HttpClient httpClient, ILogger<RoatpApplicationApiClient> logger, IRoatpApplicationTokenService tokenService) : base(httpClient, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_httpClient.BaseAddress));
        }

        // Add end points below
        public async Task<IEnumerable<ApplicationDetails>> GetOversightsPending()
        {
            return await Get<List<ApplicationDetails>>($"/Oversights/Pending");
        }

        public async Task<IEnumerable<OverallOutcomeDetails>> GetOversightsCompleted()
        {
            return await Get<List<OverallOutcomeDetails>>($"/Oversights/Completed");
        }

        public async Task<ApplicationDetails> GetOversightDetails(Guid applicationId)
        {
            return await Get<ApplicationDetails>($"/Oversights/{applicationId}");
        }
    }
}
