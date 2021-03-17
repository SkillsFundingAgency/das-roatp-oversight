﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class OversightOrchestrator : IOversightOrchestrator
    {
        private readonly ILogger<OversightOrchestrator> _logger;
        private readonly IApplyApiClient _applyApiClient;
        private readonly ICacheStorageService _cacheStorageService;

        public OversightOrchestrator(IApplyApiClient applyApiClient, ILogger<OversightOrchestrator> logger,
            ICacheStorageService cacheStorageService)
        {
            _applyApiClient = applyApiClient;
            _logger = logger;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<ApplicationsViewModel> GetApplicationsViewModel()
        {
            var result = new ApplicationsViewModel();
            var pendingApplications = await _applyApiClient.GetOversightsPending();
            var completedApplications = await _applyApiClient.GetOversightsCompleted();

            result.ApplicationDetails = pendingApplications;
            result.ApplicationCount = pendingApplications.Reviews.Count;
            result.OverallOutcomeDetails = completedApplications;
            result.OverallOutcomeCount = completedApplications.Reviews.Count;

            return result;
        }

        public async Task<OutcomeViewModel> GetOversightDetailsViewModel(Guid applicationId, Guid? outcomeKey)
        {
            var applicationDetailsTask = _applyApiClient.GetOversightDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetOversightDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;

            GetAppealResponse appealResponse = null;
            if (oversightReview != null)
            {
                appealResponse = await _applyApiClient.GetAppeal(applicationId, oversightReview.Id);
            }

            var viewModel = new OutcomeViewModel
            {
                IsNew = applicationDetails.OversightStatus == OversightReviewStatus.None,
                ApplicationSummary = CreateApplicationSummaryViewModel(applicationDetails),
                GatewayOutcome = CreateGatewayOutcomeViewModel(applicationDetails),
                FinancialHealthOutcome = CreateFinancialHealthOutcomeViewModel(applicationDetails),
                ModerationOutcome = CreateModerationOutcomeViewModel(applicationDetails),
                InProgressDetails = CreateInProgressDetailsViewModel(applicationDetails),
                OverallOutcome = CreateOverallOutcomeViewModel(oversightReview),
                AppealViewModel = appealResponse == null ? null : CreateAppealViewModel(applicationDetails, appealResponse),
                ShowAppealLink = applicationDetails.OversightStatus == OversightReviewStatus.Unsuccessful && appealResponse == null,
                ShowInProgressDetails = applicationDetails.InProgressDate.HasValue,
                OversightStatus = oversightReview?.Status ?? OversightReviewStatus.None,
                ApproveGateway = GetStringValueForApprovalStatusBoolean(applicationDetails.GatewayApproved),
                ApproveModeration = GetStringValueForApprovalStatusBoolean(applicationDetails.ModerationApproved),
                IsGatewayRemoved = applicationDetails.ApplicationStatus == ApplicationStatus.Removed,
                IsGatewayFail = applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail,
                HasFinalOutcome = oversightReview != null && oversightReview.Status != OversightReviewStatus.None && oversightReview.Status != OversightReviewStatus.InProgress
            };

            if (applicationDetails.OversightStatus == OversightReviewStatus.None || applicationDetails.OversightStatus == OversightReviewStatus.InProgress)
            {
                var cachedItem = await _cacheStorageService.RetrieveFromCache<OutcomePostRequest>(outcomeKey.ToString());
                if (cachedItem == null) return viewModel;

                viewModel.OversightStatus = cachedItem.OversightStatus;
                viewModel.ApproveGateway = cachedItem.ApproveGateway;
                viewModel.ApproveModeration = cachedItem.ApproveModeration;
                viewModel.SuccessfulText = cachedItem.SuccessfulText;
                viewModel.SuccessfulAlreadyActiveText = cachedItem.SuccessfulAlreadyActiveText;
                viewModel.SuccessfulFitnessForFundingText = cachedItem.SuccessfulFitnessForFundingText;
                viewModel.UnsuccessfulText = cachedItem.UnsuccessfulText;
                viewModel.UnsuccessfulExternalText = cachedItem.UnsuccessfulExternalText;
                viewModel.InProgressInternalText = cachedItem.InProgressInternalText;
                viewModel.InProgressExternalText = cachedItem.InProgressExternalText;
            }

            return viewModel;
        }

        private string GetStringValueForApprovalStatusBoolean(bool? approvalStatusBoolean)
        {
            if (!approvalStatusBoolean.HasValue) return string.Empty;
            return approvalStatusBoolean.Value ? ApprovalStatus.Approve : ApprovalStatus.Overturn;
        }

        public async Task<ConfirmOutcomeViewModel> GetConfirmOutcomeViewModel(Guid applicationId, Guid confirmCacheKey)
        {
            var cachedItem =
                await _cacheStorageService.RetrieveFromCache<OutcomePostRequest>(confirmCacheKey.ToString());

            if (cachedItem == null || cachedItem.ApplicationId != applicationId)
            {
                throw new ConfirmOutcomeCacheKeyNotFoundException();
            }

            var applicationDetailsTask = _applyApiClient.GetOversightDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetOversightDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;

            VerifyApplicationHasNoFinalOutcome(oversightReview);

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
                ApproveGateway = cachedItem.ApproveGateway,
                ApproveModeration = cachedItem.ApproveModeration,
                OversightStatus = cachedItem.OversightStatus
            };

            var hasAnyOverturnedOutcomes = cachedItem.ApproveGateway == ApprovalStatus.Overturn ||
                                           cachedItem.ApproveModeration == ApprovalStatus.Overturn;

            switch (cachedItem.OversightStatus)
            {
                case OversightReviewStatus.Successful:
                    viewModel.InternalComments = cachedItem.SuccessfulText;
                    break;
                case OversightReviewStatus.SuccessfulAlreadyActive:
                    viewModel.InternalComments = cachedItem.SuccessfulAlreadyActiveText;
                    break;
                case OversightReviewStatus.SuccessfulFitnessForFunding:
                    viewModel.InternalComments = cachedItem.SuccessfulFitnessForFundingText;
                    break;
                case OversightReviewStatus.InProgress:
                    viewModel.InternalComments = cachedItem.InProgressInternalText;
                    viewModel.ExternalComments = cachedItem.InProgressExternalText;
                    break;
                case OversightReviewStatus.Unsuccessful:
                    viewModel.InternalComments = cachedItem.UnsuccessfulText;
                    viewModel.ExternalComments = hasAnyOverturnedOutcomes ? cachedItem.UnsuccessfulExternalText : string.Empty;
                    break;
            }

            return viewModel;
        }

        public async Task<Guid> SaveOutcomePostRequestToCache(OutcomePostRequest request)
        {
            var key = Guid.NewGuid();
            await _cacheStorageService.SaveToCache(key.ToString(), request, 1);
            return key;
        }

        public async Task<ConfirmedViewModel> GetConfirmedViewModel(Guid applicationId)
        {
            var review = await _applyApiClient.GetOversightReview(applicationId);

            return new ConfirmedViewModel
            {
                ApplicationId = applicationId,
                OversightStatus = review.Status
            };
        }

        private void VerifyApplicationHasNoFinalOutcome(GetOversightReviewResponse oversightReview)
        {
            if (oversightReview != null && oversightReview.Status != OversightReviewStatus.None && oversightReview.Status != OversightReviewStatus.InProgress)
            {
                throw new InvalidStateException();
            }
        }

        private ApplicationSummaryViewModel CreateApplicationSummaryViewModel(ApplicationDetails applicationDetails)
        {
            var result = new ApplicationSummaryViewModel
            {
                ApplicationId = applicationDetails.ApplicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
                Ukprn = applicationDetails.Ukprn,
                ProviderRoute = applicationDetails.ProviderRoute,
                ApplicationStatus = applicationDetails.ApplicationStatus,
                ApplicationEmailAddress = applicationDetails.ApplicationEmailAddress,
                AssessorReviewStatus = applicationDetails.AssessorReviewStatus,
            };

            var financialDetailsPass = false;
            if (applicationDetails.FinancialReviewStatus == FinancialReviewStatus.Exempt)
                financialDetailsPass = true;
            else
            {
                if (applicationDetails.FinancialReviewStatus == FinancialReviewStatus.Pass &&
                    (applicationDetails.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Exempt ||
                     applicationDetails.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Outstanding ||
                     applicationDetails.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Good ||
                     applicationDetails.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Satisfactory))
                    financialDetailsPass = true;
            }

            if (applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Pass &&
                applicationDetails.ModerationReviewStatus == ModerationReviewStatus.Pass &&
                financialDetailsPass)
            {
                result.AssessmentOutcome = AssessmentOutcomeStatus.Passed;
            }
            else
            {
                result.AssessmentOutcome = AssessmentOutcomeStatus.Failed;
            }

            result.ShowAssessmentOutcome = applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Pass;

            return result;
        }

        private GatewayOutcomeViewModel CreateGatewayOutcomeViewModel(ApplicationDetails applicationDetails)
        {
            var result = new GatewayOutcomeViewModel
            {
                GatewayReviewStatus = applicationDetails.ApplicationStatus == ApplicationStatus.Removed
                    ? ApplicationStatus.Removed
                    : applicationDetails.GatewayReviewStatus,
                GatewayOutcomeMadeDate = applicationDetails.ApplicationStatus == ApplicationStatus.Removed
                    ? applicationDetails.ApplicationRemovedOn
                    : applicationDetails.GatewayOutcomeMadeDate,
                GatewayOutcomeMadeBy = applicationDetails.ApplicationStatus == ApplicationStatus.Removed
                    ? applicationDetails.ApplicationRemovedBy
                    : applicationDetails.GatewayOutcomeMadeBy,
                GatewayComments = applicationDetails.ApplicationStatus == ApplicationStatus.Removed
                    ? applicationDetails.ApplyInternalComments
                    : applicationDetails.GatewayComments,
                GatewayExternalComments = applicationDetails.ApplicationStatus == ApplicationStatus.Removed
                    ? applicationDetails.ApplyExternalComments
                    : applicationDetails.GatewayExternalComments
            };

            if (applicationDetails.GatewayApproved.HasValue)
            {
                if (applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Pass)
                {
                    result.GovernanceOutcome = applicationDetails.GatewayApproved.Value
                        ? PassFailStatus.Passed
                        : PassFailStatus.Failed;
                }
                else if (applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail)
                {
                    result.GovernanceOutcome = applicationDetails.GatewayApproved.Value
                        ? PassFailStatus.Failed
                        : PassFailStatus.Passed;
                }
            }

            return result;
        }

        private FinancialHealthOutcomeViewModel CreateFinancialHealthOutcomeViewModel(
            ApplicationDetails applicationDetails)
        {
            return new FinancialHealthOutcomeViewModel
            {
                FinancialReviewStatus = applicationDetails.FinancialReviewStatus,
                FinancialGradeAwarded = applicationDetails.FinancialGradeAwarded,
                FinancialHealthAssessedOn = applicationDetails.FinancialHealthAssessedOn,
                FinancialHealthAssessedBy = applicationDetails.FinancialHealthAssessedBy,
                FinancialHealthComments = applicationDetails.FinancialHealthComments,
                FinancialHealthExternalComments = applicationDetails.FinancialHealthExternalComments
            };
        }

        private ModerationOutcomeViewModel CreateModerationOutcomeViewModel(ApplicationDetails applicationDetails)
        {
            var result = new ModerationOutcomeViewModel
            {
                ModerationReviewStatus = applicationDetails.ModerationReviewStatus,
                ModerationOutcomeMadeOn = applicationDetails.ModerationOutcomeMadeOn,
                ModeratedBy = applicationDetails.ModeratedBy,
                ModerationComments = applicationDetails.ModerationComments
            };

            if (applicationDetails.ModerationApproved.HasValue)
            {
                if (applicationDetails.ModerationReviewStatus == ModerationReviewStatus.Pass)
                {
                    result.GovernanceOutcome = applicationDetails.ModerationApproved.Value
                        ? PassFailStatus.Passed
                        : PassFailStatus.Failed;
                }
                else if (applicationDetails.ModerationReviewStatus == ModerationReviewStatus.Fail)
                {
                    result.GovernanceOutcome = applicationDetails.ModerationApproved.Value
                        ? PassFailStatus.Failed
                        : PassFailStatus.Passed;
                }
            }

            return result;
        }

        private InProgressDetailsViewModel CreateInProgressDetailsViewModel(ApplicationDetails applicationDetails)
        {
            if (!applicationDetails.InProgressDate.HasValue) return null;

            return new InProgressDetailsViewModel
            {
                ApplicationDeterminedDate = applicationDetails.InProgressDate.Value,
                InternalComments = applicationDetails.InProgressInternalComments,
                ExternalComments = applicationDetails.InProgressExternalComments,
                UserName = applicationDetails.InProgressUserName
            };
        }

        private OverallOutcomeViewModel CreateOverallOutcomeViewModel(GetOversightReviewResponse oversightReview)
        {
            if (oversightReview == null) return new OverallOutcomeViewModel();

            return new OverallOutcomeViewModel
            {
                OversightStatus = oversightReview.Status,
                ApplicationDeterminedDate = oversightReview.ApplicationDeterminedDate,
                OversightUserName = oversightReview.UserName,
                InternalComments = oversightReview.InternalComments,
                ExternalComments = oversightReview.ExternalComments,
                IsGatewayOutcome = oversightReview.Status == OversightReviewStatus.Rejected ||
                                   oversightReview.Status == OversightReviewStatus.Withdrawn
            };
        }

        private AppealOutcomeViewModel CreateAppealViewModel(ApplicationDetails applicationDetails, GetAppealResponse appealResponse)
        {
            return new AppealOutcomeViewModel
            {
                ApplicationId = applicationDetails.ApplicationId,
                AppealId = appealResponse.Id,
                Message = appealResponse.Message,
                CreatedOn = appealResponse.CreatedOn,
                Status = appealResponse.Status,
                UserId = appealResponse.UserId,
                UserName = appealResponse.UserName,
                Uploads = appealResponse.Uploads.Select((upload => new AppealOutcomeViewModel.AppealUpload
                    {
                        Id = upload.Id,
                        Filename = upload.Filename,
                        ContentType = upload.ContentType,
                    })).ToList()
            };
        }
    }
}
