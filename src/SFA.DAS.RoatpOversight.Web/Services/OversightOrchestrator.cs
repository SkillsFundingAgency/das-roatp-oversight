using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class OversightOrchestrator:IOversightOrchestrator
    {
        private readonly ILogger<OversightOrchestrator> _logger;
        private readonly IApplyApiClient _applyApiClient;
        private readonly ICacheStorageService _cacheStorageService;

        public OversightOrchestrator(IApplyApiClient applyApiClient, ILogger<OversightOrchestrator> logger, ICacheStorageService cacheStorageService)
        {
            _applyApiClient = applyApiClient;
            _logger = logger;
            _cacheStorageService = cacheStorageService;
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

        public async Task<OutcomeViewModel> GetOversightDetailsViewModel(Guid applicationId, Guid? outcomeKey)
        {
            var applicationDetails = await _applyApiClient.GetOversightDetails(applicationId);
            var cachedItem = await _cacheStorageService.RetrieveFromCache<OutcomePostRequest>(outcomeKey.ToString());

            var viewModel = new OutcomeViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
                Ukprn = applicationDetails.Ukprn,
                ProviderRoute = applicationDetails.ProviderRoute,
                OversightStatus = applicationDetails.OversightStatus,
                ApplicationStatus = applicationDetails.ApplicationStatus,
                ApplicationEmailAddress = applicationDetails.ApplicationEmailAddress,
                AssessorReviewStatus = applicationDetails.AssessorReviewStatus,
                GatewayReviewStatus = applicationDetails.GatewayReviewStatus,
                GatewayOutcomeMadeDate = applicationDetails.GatewayOutcomeMadeDate,
                GatewayOutcomeMadeBy = applicationDetails.GatewayOutcomeMadeBy,
                GatewayComments = applicationDetails.GatewayComments,
                FinancialReviewStatus = applicationDetails.FinancialReviewStatus,
                FinancialGradeAwarded = applicationDetails.FinancialGradeAwarded,
                FinancialHealthAssessedOn =  applicationDetails.FinancialHealthAssessedOn,
                FinancialHealthAssessedBy = applicationDetails.FinancialHealthAssessedBy,
                FinancialHealthComments = applicationDetails.FinancialHealthComments,
                ModerationReviewStatus = applicationDetails.ModerationReviewStatus,
                ModerationOutcomeMadeOn = applicationDetails.ModerationOutcomeMadeOn,
                ModeratedBy = applicationDetails.ModeratedBy,
                ModerationComments = applicationDetails.ModerationComments
            };

            if (cachedItem != null)
            {
                viewModel.OversightStatus = cachedItem.OversightStatus;
                viewModel.ApproveGateway = cachedItem.ApproveGateway;
                viewModel.ApproveModeration = cachedItem.ApproveModeration;
                viewModel.SuccessfulText = cachedItem.SuccessfulText;
                viewModel.SuccessfulAlreadyActiveText = cachedItem.SuccessfulAlreadyActiveText;
                viewModel.SuccessfulFitnessForFundingText = cachedItem.SuccessfulFitnessForFundingText;
                viewModel.UnsuccessfulText = cachedItem.UnsuccessfulText;
                viewModel.InProgressInternalText = cachedItem.InProgressInternalText;
                viewModel.InProgressExternalText = cachedItem.InProgressExternalText;
            }

            return viewModel;
        }

        public async Task<ConfirmOutcomeViewModel> GetConfirmOutcomeViewModel(Guid applicationId, Guid confirmCacheKey)
        {
            var cachedItem = await _cacheStorageService.RetrieveFromCache<OutcomePostRequest>(confirmCacheKey.ToString());

            if (cachedItem == null || cachedItem.ApplicationId != applicationId)
            {
                throw new ConfirmOutcomeCacheKeyNotFound();
            }

            var applicationDetails = await _applyApiClient.GetOversightDetails(applicationId);

            var viewModel = new ConfirmOutcomeViewModel
            {
                ApplicationId = applicationId,
                OutcomeKey = confirmCacheKey,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
                Ukprn = applicationDetails.Ukprn,
                ProviderRoute = applicationDetails.ProviderRoute,

                ApplicationStatus = applicationDetails.ApplicationStatus,
                ApplicationEmailAddress = applicationDetails.ApplicationEmailAddress,
                AssessorReviewStatus = applicationDetails.AssessorReviewStatus,
                GatewayReviewStatus = applicationDetails.GatewayReviewStatus,
                GatewayOutcomeMadeDate = applicationDetails.GatewayOutcomeMadeDate,
                GatewayOutcomeMadeBy = applicationDetails.GatewayOutcomeMadeBy,
                GatewayComments = applicationDetails.GatewayComments,
                FinancialReviewStatus = applicationDetails.FinancialReviewStatus,
                FinancialGradeAwarded = applicationDetails.FinancialGradeAwarded,
                FinancialHealthAssessedOn = applicationDetails.FinancialHealthAssessedOn,
                FinancialHealthAssessedBy = applicationDetails.FinancialHealthAssessedBy,
                FinancialHealthComments = applicationDetails.FinancialHealthComments,
                ModerationReviewStatus = applicationDetails.ModerationReviewStatus,
                ModerationOutcomeMadeOn = applicationDetails.ModerationOutcomeMadeOn,
                ModeratedBy = applicationDetails.ModeratedBy,
                ModerationComments = applicationDetails.ModerationComments,


                OversightStatus = cachedItem.OversightStatus,
                ApproveGateway = cachedItem.ApproveGateway,
                ApproveModeration = cachedItem.ApproveModeration,
                SuccessfulText = cachedItem.SuccessfulText,
                SuccessfulAlreadyActiveText = cachedItem.SuccessfulAlreadyActiveText,
                SuccessfulFitnessForFundingText = cachedItem.SuccessfulFitnessForFundingText,
                UnsuccessfulText = cachedItem.UnsuccessfulText,
                InProgressInternalText = cachedItem.InProgressInternalText,
                InProgressExternalText = cachedItem.InProgressExternalText
            };

            return viewModel;
        }

        public async Task<Guid> SaveOutcomePostRequestToCache(OutcomePostRequest request)
        { 
            var key = Guid.NewGuid();
            await _cacheStorageService.SaveToCache(key.ToString(), request, 1);
            return key;
        }
    }
}
