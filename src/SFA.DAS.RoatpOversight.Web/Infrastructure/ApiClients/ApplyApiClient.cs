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
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class ApplyApiClient : ApiClientBase<ApplyApiClient>, IApplyApiClient
    {
        private readonly IMultipartFormDataService _multipartFormDataService;

        public ApplyApiClient(HttpClient httpClient,
            ILogger<ApplyApiClient> logger,
            IRoatpApplicationTokenService tokenService,
            IMultipartFormDataService multipartFormDataService) : base(httpClient,
            logger)
        {
            _multipartFormDataService = multipartFormDataService;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_httpClient.BaseAddress));
        }

        public async Task Ping()
        {
            await Get<string>($"/ping");
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

        public async Task RecordGatewayRemovedOutcome(RecordOversightGatewayRemovedOutcomeCommand command)
        {
            await Post("Oversight/GatewayRemovedOutcome", command);
        }
		
        public async Task UploadAppealFile(Guid applicationId, UploadAppealFileRequest request)
        {
            await PostMultipartAsync($"/Oversight/{applicationId}/uploads", request);
        }

        public async Task RemoveAppealFile(Guid applicationId, Guid fileId, RemoveAppealFileCommand command)
        {
            await Post($"Oversight/{applicationId}/uploads/{fileId}/remove", command);
        }

        public async Task<AppealFiles> GetStagedUploads(GetStagedFilesRequest request)
        {
            return await Get<AppealFiles>($"Oversight/{request.ApplicationId}/uploads");
        }

        private async Task PostMultipartAsync(string requestUri, object request)
        {
            var content = _multipartFormDataService.CreateMultipartFormDataContent(request);
            await _httpClient.PostAsync(requestUri, content);
        }
    }
}
