using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IApplicationOutcomeOrchestrator
    {
        Task<bool> RecordOutcome(Guid applicationId, bool? approveGateway, bool? approveModerator, OversightReviewStatus outcome, string userId, string userName, string internalComments, string externalComments);
    }
}
