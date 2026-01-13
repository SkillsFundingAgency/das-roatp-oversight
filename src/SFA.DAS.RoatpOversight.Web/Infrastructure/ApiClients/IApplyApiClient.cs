using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

public interface IApplyApiClient
{
    [Get("/ping")]
    Task Ping();

    [Get("/Oversights/Pending")]
    Task<PendingOversightReviews> GetOversightsPending([Query] string searchTerm, [Query] string sortColumn, [Query] string sortOrder);

    [Get("/Oversights/Completed")]
    Task<CompletedOversightReviews> GetOversightsCompleted([Query] string searchTerm, [Query] string sortColumn, [Query] string sortOrder);

    [Get("/Oversights/PendingAppeal")]
    Task<PendingAppealOutcomes> GetPendingAppealOutcomes([Query] string searchTerm, [Query] string sortColumn, [Query] string sortOrder);

    [Get("/Oversights/CompletedAppeal")]
    Task<CompletedAppealOutcomes> GetCompletedAppealOutcomesCompleted([Query] string searchTerm, [Query] string sortColumn, [Query] string sortOrder);

    [Get("/Oversights/{applicationId}")]
    Task<ApplicationDetails> GetApplicationDetails(Guid applicationId);

    [Get("/Oversight/RegistrationDetails/{applicationId}")]
    Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);

    [Post("/Oversight/Outcome")]
    Task<bool> RecordOutcome([Body] RecordOversightOutcomeCommand command);

    [Post("/Oversight/Appeal")]
    Task<bool> RecordAppeal([Body] RecordAppealOutcomeCommand command);

    [Post("/Oversight/GatewayFailOutcome")]
    Task RecordGatewayFailOutcome([Body] RecordOversightGatewayFailOutcomeCommand command);

    [Post("/Oversight/GatewayRemovedOutcome")]
    Task RecordGatewayRemovedOutcome([Body] RecordOversightGatewayRemovedOutcomeCommand command);

    [Get("/Oversight/{applicationId}/review")]
    Task<GetOversightReviewResponse> GetOversightReview(Guid applicationId);

    [Get("/Appeals/{applicationId")]
    Task<AppealDetails> GetAppealDetails(Guid applicationId);

    [Get("/Appeals/{applicationId}/files/{fileName}")]
    Task<HttpResponseMessage> DownloadFile(Guid applicationId, string fileName);
}
