using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IRoatpApplicationApiClient
    {
        Task<RoatpRegistrationDetails> GetRegistrationDetails(Guid applicationId);

        Task<bool> RecordOutcome(RecordOversightOutcomeCommand command);
    }
}
