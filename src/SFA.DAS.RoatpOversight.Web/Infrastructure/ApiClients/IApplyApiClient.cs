using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task Ping();
        Task<IEnumerable<ApplicationDetails>> GetOversightsPending();
        Task<IEnumerable<OverallOutcomeDetails>> GetOversightsCompleted();
        Task<ApplicationDetails> GetOversightDetails(Guid applicationId);
        Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);
        Task<bool> RecordOutcome(RecordOversightOutcomeCommand command);
    }
}
