using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IOversightOrchestrator
    {
        Task<ApplicationsViewModel> GetOversightOverviewViewModel();
        Task<OutcomeViewModel> GetOversightDetailsViewModel(Guid applicationId, Guid? outcomeKey);

        Task<ConfirmOutcomeViewModel> GetConfirmOutcomeViewModel(Guid applicationId, Guid confirmCacheKey);
        Task<Guid> SaveOutcomePostRequestToCache(OutcomePostRequest request);
        Task<ConfirmedViewModel> GetConfirmedViewModel(Guid applicationId);
    }
}

