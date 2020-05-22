using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using System.Net;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class ApplyApiClient : ApiClientBase<ApplyApiClient>, IApplyApiClient
    {
        public ApplyApiClient(HttpClient httpClient, ILogger<ApplyApiClient> logger, IRoatpApplicationTokenService tokenService) : base(httpClient, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_httpClient.BaseAddress));
        }

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
          public async Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId)
        {
            return await Get<RoatpRegistrationDetails>($"Oversight/RegistrationDetails/{applicationId}");
        }

        public async Task<bool> RecordOutcome(RecordOversightOutcomeCommand command)
        {
            var statusCode = await Post("Oversight/Outcome", command);

            return await Task.FromResult(statusCode == HttpStatusCode.OK);
        }
    }
}
