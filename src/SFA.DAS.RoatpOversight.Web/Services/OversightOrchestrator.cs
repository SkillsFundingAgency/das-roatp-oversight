using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class OversightOrchestrator:IOversightOrchestrator
    {
        private readonly ILogger<OversightOrchestrator> _logger;
        private readonly IRoatpApplicationApiClient _applyApiClient;

        public OversightOrchestrator(IRoatpApplicationApiClient applyApiClient, ILogger<OversightOrchestrator> logger)
        {
            _applyApiClient = applyApiClient;
            _logger = logger;
        }

        public async Task<OverallOutcomeViewModel> GetOversightOverviewViewModel()
        {
           var viewModel = new OverallOutcomeViewModel();
           var pendingApplications = await _applyApiClient.GetOversightsPending();
           var completedApplications = await _applyApiClient.GetOversightsCompleted();

           viewModel.ApplicationDetails = pendingApplications==null ? new List<ApplicationDetails>() : pendingApplications.ToList();

           viewModel.ApplicationCount = viewModel.ApplicationDetails.Count;

           viewModel.OverallOutcomeDetails = completedApplications == null ? new List<OverallOutcomeDetails>() : completedApplications.ToList();

           viewModel.OverallOutcomeCount = viewModel.OverallOutcomeDetails.Count;

           return viewModel;
        }

        public async Task<OutcomeViewModel> GetOversightDetailsViewModel(Guid applicationId)
        {
            var applicationDetails = await _applyApiClient.GetOversightDetails(applicationId);

            var viewModel = new OutcomeViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
                Ukprn = applicationDetails.Ukprn,
                ProviderRoute = applicationDetails.ProviderRoute
            };


            return viewModel;
        }
    }
}
