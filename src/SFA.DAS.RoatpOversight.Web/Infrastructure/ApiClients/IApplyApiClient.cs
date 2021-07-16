using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task Ping();
        Task<PendingOversightReviews> GetOversightsPending(string sortColumn, string sortOrder);
        Task<CompletedOversightReviews> GetOversightsCompleted(string sortColumn, string sortOrder);
        Task<ApplicationDetails> GetApplicationDetails(Guid applicationId);
        Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);
        Task<bool> RecordOutcome(RecordOversightOutcomeCommand command);
        Task RecordGatewayFailOutcome(RecordOversightGatewayFailOutcomeCommand command);
        Task RecordGatewayRemovedOutcome(RecordOversightGatewayRemovedOutcomeCommand command);
        Task UploadAppealFile(Guid applicationId, UploadAppealFileRequest request);
        Task RemoveAppealFile(Guid applicationId, Guid fileId, RemoveAppealFileCommand command);
        Task<AppealFiles> GetStagedUploads(GetStagedFilesRequest request);
        Task CreateAppeal(Guid applicationId, Guid oversightReviewId, CreateAppealRequest request);
        Task<GetAppealResponse> GetAppeal(Guid applicationId, Guid oversightReviewId);
        Task<GetAppealUploadResponse> GetAppealFile(Guid applicationId, Guid appealId, Guid fileId);
        Task<GetOversightReviewResponse> GetOversightReview(Guid applicationId);
    }
}
