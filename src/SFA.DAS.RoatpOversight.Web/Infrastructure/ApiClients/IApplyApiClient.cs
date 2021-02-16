using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task Ping();
        Task<PendingOversightReviews> GetOversightsPending();
        Task<CompletedOversightReviews> GetOversightsCompleted();
        Task<ApplicationDetails> GetOversightDetails(Guid applicationId);
        Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);
        Task<bool> RecordOutcome(RecordOversightOutcomeCommand command);
        Task RecordGatewayFailOutcome(RecordOversightGatewayFailOutcomeCommand command);
        Task RecordGatewayRemovedOutcome(RecordOversightGatewayRemovedOutcomeCommand command);
    }
}
