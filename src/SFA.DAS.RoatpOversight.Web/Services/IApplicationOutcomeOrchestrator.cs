using System;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IApplicationOutcomeOrchestrator
    {
        Task<bool> RecordOutcome(Guid applicationId, string outcome, string updatedBy);
    }
}
