using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task Ping();
        Task<PendingOversightReviews> GetOversightsPending(string searchTerm, string sortColumn, string sortOrder);
        Task<CompletedOversightReviews> GetOversightsCompleted(string searchTerm, string sortColumn, string sortOrder);
        Task<ApplicationDetails> GetApplicationDetails(Guid applicationId);
        Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);
        Task<bool> RecordOutcome(RecordOversightOutcomeCommand command);
        Task RecordGatewayFailOutcome(RecordOversightGatewayFailOutcomeCommand command);
        Task RecordGatewayRemovedOutcome(RecordOversightGatewayRemovedOutcomeCommand command);
        Task<GetOversightReviewResponse> GetOversightReview(Guid applicationId);

        Task<AppealDetails> GetAppealDetails(Guid applicationId);
    }
}
