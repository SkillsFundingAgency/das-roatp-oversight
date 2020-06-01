using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IOversightOrchestrator
    {
        Task<OverallOutcomeViewModel> GetOversightOverviewViewModel();
        Task<OutcomeViewModel> GetOversightDetailsViewModel(Guid applicationId);
    }
}

