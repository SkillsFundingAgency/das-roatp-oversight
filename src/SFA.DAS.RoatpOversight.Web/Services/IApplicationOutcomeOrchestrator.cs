using System;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IApplicationOutcomeOrchestrator
    {
        Task<bool> RecordOutcome(Guid applicationId, bool? approveGateway, bool? approveModerator, string outcome, string userId, string userName, string internalComments, string externalComments);
    }
}
