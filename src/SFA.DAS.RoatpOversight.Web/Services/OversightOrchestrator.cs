using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly IRoatpRegisterApiClient _registerApiClient;

        private readonly ICacheStorageService _cacheStorageService;

        public OversightOrchestrator(IApplyApiClient applyApiClient, ILogger<OversightOrchestrator> logger,
            ICacheStorageService cacheStorageService, IRoatpRegisterApiClient registerApiClient)
        {
            _applyApiClient = applyApiClient;
            _logger = logger;
            _cacheStorageService = cacheStorageService;
            _registerApiClient = registerApiClient;
        }

        public async Task<ApplicationsViewModel> GetApplicationsViewModel(string selectedTab, string searchTerm, string sortColumn, string sortOrder)
        {    
            var pendingApplications = await _applyApiClient.GetOversightsPending(searchTerm, sortColumn, sortOrder);
            var completedApplications = await _applyApiClient.GetOversightsCompleted(searchTerm, sortColumn, sortOrder);

            var result = new ApplicationsViewModel
            {
                ApplicationDetails = pendingApplications,
                ApplicationCount = pendingApplications.Reviews.Count,
                OverallOutcomeDetails = completedApplications,
                OverallOutcomeCount = completedApplications.Reviews.Count,
                SelectedTab = selectedTab,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder
            };

            return result;
        }

        public async Task<AppealViewModel> GetAppealDetailsViewModel(Guid applicationId, Guid? outcomeKey)
        {
            var applicationDetailsTask = _applyApiClient.GetApplicationDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetApplicationDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;
            var onRegister = false;
            var appealDetails = await _applyApiClient.GetAppealDetails(applicationId);
   

            if (applicationDetails?.Ukprn != null)
            {
                var registerStatus = await _registerApiClient.GetOrganisationRegisterStatus(new GetOrganisationRegisterStatusRequest { UKPRN = applicationDetails.Ukprn });
                onRegister = registerStatus.UkprnOnRegister;
            }


            var viewModel = new AppealViewModel
            {
                IsNew = oversightReview == null,
                ApplicationSummary = CreateApplicationSummaryViewModel(applicationDetails),
                GatewayOutcome = CreateGatewayOutcomeViewModel(applicationDetails, oversightReview),
                FinancialHealthOutcome = CreateFinancialHealthOutcomeViewModel(applicationDetails),
                ModerationOutcome = CreateModerationOutcomeViewModel(applicationDetails, oversightReview),
                InProgressDetails = CreateInProgressDetailsViewModel(oversightReview),
                OverallOutcome = CreateOverallOutcomeViewModel(oversightReview),
                ShowInProgressDetails = oversightReview?.InProgressDate != null,
                OversightStatus = oversightReview?.Status ?? OversightReviewStatus.None,
                ApproveGateway = GetStringValueForApprovalStatusBoolean(oversightReview?.GatewayApproved),
                ApproveModeration = GetStringValueForApprovalStatusBoolean(oversightReview?.ModerationApproved),
                IsGatewayRemoved = applicationDetails.ApplicationStatus == ApplicationStatus.Removed,
                IsGatewayFail = applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail,
                HasFinalOutcome = oversightReview != null && oversightReview.Status != OversightReviewStatus.None && oversightReview.Status != OversightReviewStatus.InProgress,
                OnRegister = onRegister,
                Appeal = appealDetails,
                AppealStatus = appealDetails.Status
            };

            if (oversightReview == null || oversightReview.Status == OversightReviewStatus.InProgress)
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


        public async Task<OutcomeDetailsViewModel> GetOversightDetailsViewModel(Guid applicationId, Guid? outcomeKey)
        {
            var applicationDetailsTask = _applyApiClient.GetApplicationDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetApplicationDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;
            var onRegister = false;

            if (applicationDetails?.Ukprn != null)
            {
                var registerStatus = await _registerApiClient.GetOrganisationRegisterStatus(new GetOrganisationRegisterStatusRequest { UKPRN = applicationDetails.Ukprn });
                onRegister = registerStatus.UkprnOnRegister;
            }


            var viewModel = new OutcomeDetailsViewModel
            {
                IsNew = oversightReview == null,
                ApplicationSummary = CreateApplicationSummaryViewModel(applicationDetails),
                GatewayOutcome = CreateGatewayOutcomeViewModel(applicationDetails, oversightReview),
                FinancialHealthOutcome = CreateFinancialHealthOutcomeViewModel(applicationDetails),
                ModerationOutcome = CreateModerationOutcomeViewModel(applicationDetails, oversightReview),
                InProgressDetails = CreateInProgressDetailsViewModel(oversightReview),
                OverallOutcome = CreateOverallOutcomeViewModel(oversightReview),
                ShowInProgressDetails = oversightReview?.InProgressDate != null,
                OversightStatus = oversightReview?.Status ?? OversightReviewStatus.None,
                ApproveGateway = GetStringValueForApprovalStatusBoolean(oversightReview?.GatewayApproved),
                ApproveModeration = GetStringValueForApprovalStatusBoolean(oversightReview?.ModerationApproved),
                IsGatewayRemoved = applicationDetails.ApplicationStatus == ApplicationStatus.Removed,
                IsGatewayFail = applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail,
                HasFinalOutcome = oversightReview != null && oversightReview.Status != OversightReviewStatus.None && oversightReview.Status != OversightReviewStatus.InProgress,
                OnRegister = onRegister
            };

            if (oversightReview == null || oversightReview.Status == OversightReviewStatus.InProgress)
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


        public async Task<OutcomeViewModel> GetOversightViewModel(Guid applicationId, Guid? outcomeKey)
        {
            var applicationDetailsTask = _applyApiClient.GetApplicationDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetApplicationDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;
            var onRegister = false;
            
            if (applicationDetails?.Ukprn != null)
            {
                var registerStatus = await _registerApiClient.GetOrganisationRegisterStatus(new GetOrganisationRegisterStatusRequest { UKPRN = applicationDetails.Ukprn });
                onRegister = registerStatus.UkprnOnRegister;
            }


            var viewModel = new OutcomeViewModel
            {
                IsNew = oversightReview == null,
                ApplicationSummary = CreateApplicationSummaryViewModel(applicationDetails),
                GatewayOutcome = CreateGatewayOutcomeViewModel(applicationDetails, oversightReview),
                FinancialHealthOutcome = CreateFinancialHealthOutcomeViewModel(applicationDetails),
                ModerationOutcome = CreateModerationOutcomeViewModel(applicationDetails, oversightReview),
                InProgressDetails = CreateInProgressDetailsViewModel(oversightReview),
                OverallOutcome = CreateOverallOutcomeViewModel(oversightReview),
                ShowInProgressDetails = oversightReview?.InProgressDate != null,
                OversightStatus = oversightReview?.Status ?? OversightReviewStatus.None,
                ApproveGateway = GetStringValueForApprovalStatusBoolean(oversightReview?.GatewayApproved),
                ApproveModeration = GetStringValueForApprovalStatusBoolean(oversightReview?.ModerationApproved),
                IsGatewayRemoved = applicationDetails.ApplicationStatus == ApplicationStatus.Removed,
                IsGatewayFail = applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail,
                HasFinalOutcome = oversightReview != null && oversightReview.Status != OversightReviewStatus.None && oversightReview.Status != OversightReviewStatus.InProgress,
                OnRegister = onRegister
            };

            if (oversightReview == null || oversightReview.Status == OversightReviewStatus.InProgress)
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

            var applicationDetailsTask = _applyApiClient.GetApplicationDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetApplicationDetails(applicationId).Result;
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



        public async Task<ConfirmAppealViewModel> GetConfirmAppealViewModel(Guid applicationId, Guid confirmCacheKey)
        {
            var cachedItem =
                await _cacheStorageService.RetrieveFromCache<AppealPostRequest>(confirmCacheKey.ToString());

            if (cachedItem == null || cachedItem.ApplicationId != applicationId)
            {
                throw new ConfirmOutcomeCacheKeyNotFoundException();
            }

            var applicationDetailsTask = _applyApiClient.GetApplicationDetails(applicationId);
            var oversightReviewTask = _applyApiClient.GetOversightReview(applicationId);
            await Task.WhenAll(applicationDetailsTask, oversightReviewTask);
            var applicationDetails = _applyApiClient.GetApplicationDetails(applicationId).Result;
            var oversightReview = _applyApiClient.GetOversightReview(applicationId).Result;

            var viewModel = new ConfirmAppealViewModel
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
                AppealStatus = cachedItem.AppealStatus
            };

                 switch (cachedItem.AppealStatus)
            {
                case AppealStatus.Successful:
                    viewModel.InternalComments = cachedItem.SuccessfulText;
                    break;
                case AppealStatus.SuccessfulAlreadyActive:
                    viewModel.InternalComments = cachedItem.SuccessfulAlreadyActiveText;
                    break;
                case AppealStatus.SuccessfulFitnessForFunding:
                    viewModel.InternalComments = cachedItem.SuccessfulFitnessForFundingText;
                    break;
                case AppealStatus.InProgress:
                    viewModel.InternalComments = cachedItem.InProgressInternalText;
                    viewModel.ExternalComments = cachedItem.InProgressExternalText;
                    break;
                case AppealStatus.Unsuccessful:
                    viewModel.InternalComments = cachedItem.UnsuccessfulText;
                    viewModel.ExternalComments = cachedItem.UnsuccessfulExternalText;
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

        public async Task<Guid> SaveAppealPostRequestToCache(AppealPostRequest request)
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

        private GatewayOutcomeViewModel CreateGatewayOutcomeViewModel(ApplicationDetails applicationDetails, GetOversightReviewResponse oversightReview)
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

            if (oversightReview?.GatewayApproved != null)
            {
                if (applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Pass)
                {
                    result.GovernanceOutcome = oversightReview.GatewayApproved.Value
                        ? PassFailStatus.Passed
                        : PassFailStatus.Failed;
                }
                else if (applicationDetails.GatewayReviewStatus == GatewayReviewStatus.Fail)
                {
                    result.GovernanceOutcome = oversightReview.GatewayApproved.Value
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

        private ModerationOutcomeViewModel CreateModerationOutcomeViewModel(ApplicationDetails applicationDetails, GetOversightReviewResponse oversightReview)
        {
            var result = new ModerationOutcomeViewModel
            {
                ModerationReviewStatus = applicationDetails.ModerationReviewStatus,
                ModerationOutcomeMadeOn = applicationDetails.ModerationOutcomeMadeOn,
                ModeratedBy = applicationDetails.ModeratedBy,
                ModerationComments = applicationDetails.ModerationComments
            };

            if (oversightReview?.ModerationApproved != null)
            {
                if (applicationDetails.ModerationReviewStatus == ModerationReviewStatus.Pass)
                {
                    result.GovernanceOutcome = oversightReview.ModerationApproved.Value
                        ? PassFailStatus.Passed
                        : PassFailStatus.Failed;
                }
                else if (applicationDetails.ModerationReviewStatus == ModerationReviewStatus.Fail)
                {
                    result.GovernanceOutcome = oversightReview.ModerationApproved.Value
                        ? PassFailStatus.Failed
                        : PassFailStatus.Passed;
                }
            }

            return result;
        }

        private InProgressDetailsViewModel CreateInProgressDetailsViewModel(GetOversightReviewResponse oversightReview)
        {
            if (oversightReview?.InProgressDate == null) return null;

            return new InProgressDetailsViewModel
            {
                ApplicationDeterminedDate = oversightReview.InProgressDate.Value,
                InternalComments = oversightReview.InProgressInternalComments,
                ExternalComments = oversightReview.InProgressExternalComments,
                UserName = oversightReview.InProgressUserName
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

       
    }
}
