using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using System.Net;
using SFA.DAS.AdminService.Common.Infrastructure;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class ApplyApiClient : ApiClientBase<ApplyApiClient>, IApplyApiClient
    {
        public ApplyApiClient(HttpClient httpClient, ILogger<ApplyApiClient> logger, IRoatpApplicationTokenService tokenService) : base(httpClient, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_httpClient.BaseAddress));
        }

        public async Task Ping()
        {
            await Get($"/ping");
        }

        public async Task<PendingOversightReviews> GetOversightsPending()
        {
            return await Get<PendingOversightReviews>($"/Oversights/Pending");
        }

        public async Task<CompletedOversightReviews> GetOversightsCompleted()
        {
            return await Get<CompletedOversightReviews>($"/Oversights/Completed");
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

        public async Task RecordGatewayFailOutcome(RecordOversightGatewayFailOutcomeCommand command)
        {
            await Post("Oversight/GatewayFailOutcome", command);
        }

        public async Task UploadAppealFile(UploadAppealFileCommand command)
        {
            await Post("Oversight/Appeal/Upload", command);
        }
    }
}
