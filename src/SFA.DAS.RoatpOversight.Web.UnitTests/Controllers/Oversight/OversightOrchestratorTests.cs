﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Settings;


namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightOrchestratorTests
    {
        private OversightOrchestrator _orchestrator;
        private Mock<IApplyApiClient> _apiClient;
        private Mock<IWebConfiguration> _configuration;
        private Mock<ICacheStorageService> _cacheStorageService;
        private Mock<IRoatpRegisterApiClient> _roatpRegisterClient;
        private string _dashboardAddress;
        private Guid _applicationId;
        private Guid _oversightReviewId;
        private readonly Fixture _autoFixture = new Fixture();
        private bool onRegister;
        private AppealDetails _appealDetails;

        [SetUp]
        public void SetUp()
        {
            _applicationId = Guid.NewGuid();
            _oversightReviewId = Guid.NewGuid();
            _apiClient = new Mock<IApplyApiClient>();
            _roatpRegisterClient = new Mock<IRoatpRegisterApiClient>();
            _configuration = new Mock<IWebConfiguration>();
            _cacheStorageService = new Mock<ICacheStorageService>();
            _dashboardAddress = "https://dashboard";
            _appealDetails= new AppealDetails {Status = AppealStatus.Submitted, 
                AppealSubmittedDate = DateTime.Today, 
                HowFailedOnEvidenceSubmitted = "how failed evidence",
                AppealFiles = new List<AppealFile> {new AppealFile {Filename ="file.pdf"}}
            };
            _orchestrator = new OversightOrchestrator(_apiClient.Object, Mock.Of<ILogger<OversightOrchestrator>>(),
                _cacheStorageService.Object, _roatpRegisterClient.Object);
        }

        [Test]
        public async Task Orchestrator_builds_overview_viewmodel_from_api()
        {
            var expectedApplicationsPending = GetApplicationsPending();
            _apiClient.Setup(x => x.GetOversightsPending(null,null,null)).ReturnsAsync(expectedApplicationsPending);
            _apiClient.Setup(x => x.GetOversightsCompleted(null,null,null)).ReturnsAsync(GetApplicationsDone());
            _apiClient.Setup(x => x.GetPendingAppealOutcomes(null, null, null)).ReturnsAsync(GetPendingAppealApplications());
            _apiClient.Setup(x => x.GetCompletedAppealOutcomesCompleted(null, null, null)).ReturnsAsync(GetCompletedApplicationsDone());
            _configuration.Setup(x => x.EsfaAdminServicesBaseUrl).Returns(_dashboardAddress);
            var actualViewModel = await _orchestrator.GetApplicationsViewModel(null,null,null,null);

            var expectedViewModel = new ApplicationsViewModel
            {
                ApplicationDetails = GetApplicationsPending(), OverallOutcomeDetails = GetApplicationsDone(),PendingAppealsDetails = GetPendingAppealApplications(), CompleteAppealsDetails = GetCompletedApplicationsDone()
            };
            expectedViewModel.ApplicationCount = expectedViewModel.ApplicationDetails.Reviews.Count;
            expectedViewModel.OverallOutcomeCount = expectedViewModel.OverallOutcomeDetails.Reviews.Count;
            expectedViewModel.AppealsCount = expectedViewModel.PendingAppealsDetails.Reviews.Count;
            expectedViewModel.AppealsOutcomeCount = expectedViewModel.CompleteAppealsDetails.Reviews.Count;

            Assert.AreEqual(actualViewModel.ApplicationCount, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeCount, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.AppealsCount, expectedViewModel.AppealsCount);
            Assert.AreEqual(actualViewModel.AppealsOutcomeCount, expectedViewModel.AppealsOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.Reviews.Count, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.Reviews.Count, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.PendingAppealsDetails.Reviews.Count, expectedViewModel.AppealsCount);
            Assert.AreEqual(actualViewModel.CompleteAppealsDetails.Reviews.Count, expectedViewModel.AppealsOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.Reviews.First().ApplicationId,
                expectedViewModel.ApplicationDetails.Reviews.First().ApplicationId);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.Reviews.First().Ukprn,
                expectedViewModel.OverallOutcomeDetails.Reviews.First().Ukprn);
            Assert.AreEqual(actualViewModel.PendingAppealsDetails.Reviews.First().ApplicationId,
               expectedViewModel.PendingAppealsDetails.Reviews.First().ApplicationId);
            Assert.AreEqual(actualViewModel.CompleteAppealsDetails.Reviews.First().Ukprn,
                expectedViewModel.CompleteAppealsDetails.Reviews.First().Ukprn);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task GetOversightDetails_returns_viewmodel(bool onRegister)
        {
            var expectedApplicationDetails = GetApplication();
            var expectedOversightReview = GetOversightReview();
            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId)).ReturnsAsync(expectedApplicationDetails);
            _apiClient.Setup(x => x.GetOversightReview(_applicationId)).ReturnsAsync(() => expectedOversightReview);
            _apiClient.Setup(x => x.GetAppealDetails(_applicationId)).ReturnsAsync(_appealDetails);
            _roatpRegisterClient
                .Setup(x => x.GetOrganisationRegisterStatus(It.IsAny<GetOrganisationRegisterStatusRequest>()))
                .ReturnsAsync(new OrganisationRegisterStatus { UkprnOnRegister = onRegister });

            var actualViewModel = await _orchestrator.GetOversightDetailsViewModel(_applicationId, null);

            Assert.AreEqual(expectedApplicationDetails.ApplicationId, _applicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationId, actualViewModel.ApplicationSummary.ApplicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationReferenceNumber,
                actualViewModel.ApplicationSummary.ApplicationReferenceNumber);
            Assert.AreEqual(expectedApplicationDetails.ApplicationSubmittedDate,
                actualViewModel.ApplicationSummary.ApplicationSubmittedDate);
            Assert.AreEqual(expectedApplicationDetails.OrganisationName,
                actualViewModel.ApplicationSummary.OrganisationName);
            Assert.AreEqual(expectedApplicationDetails.ProviderRoute, actualViewModel.ApplicationSummary.ProviderRoute);
            Assert.AreEqual(expectedApplicationDetails.Ukprn, actualViewModel.ApplicationSummary.Ukprn);
            Assert.AreEqual(expectedOversightReview.Status, actualViewModel.OversightStatus);
            Assert.AreEqual(expectedApplicationDetails.ApplicationStatus,
                actualViewModel.ApplicationSummary.ApplicationStatus);
            Assert.AreEqual(expectedApplicationDetails.ApplicationEmailAddress,
                actualViewModel.ApplicationSummary.ApplicationEmailAddress);
            Assert.AreEqual(expectedApplicationDetails.AssessorReviewStatus,
                actualViewModel.ApplicationSummary.AssessorReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.GatewayReviewStatus,
                actualViewModel.GatewayOutcome.GatewayReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeDate,
                actualViewModel.GatewayOutcome.GatewayOutcomeMadeDate);
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeBy,
                actualViewModel.GatewayOutcome.GatewayOutcomeMadeBy);
            Assert.AreEqual(expectedApplicationDetails.GatewayComments, actualViewModel.GatewayOutcome.GatewayComments);
            Assert.AreEqual(expectedApplicationDetails.FinancialReviewStatus,
                actualViewModel.FinancialHealthOutcome.FinancialReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.FinancialGradeAwarded,
                actualViewModel.FinancialHealthOutcome.FinancialGradeAwarded);
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedOn,
                actualViewModel.FinancialHealthOutcome.FinancialHealthAssessedOn);
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedBy,
                actualViewModel.FinancialHealthOutcome.FinancialHealthAssessedBy);
            Assert.AreEqual(expectedApplicationDetails.ModerationReviewStatus,
                actualViewModel.ModerationOutcome.ModerationReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.ModerationOutcomeMadeOn,
                actualViewModel.ModerationOutcome.ModerationOutcomeMadeOn);
            Assert.AreEqual(expectedApplicationDetails.ModeratedBy, actualViewModel.ModerationOutcome.ModeratedBy);
            Assert.AreEqual(expectedApplicationDetails.ModerationComments,
                actualViewModel.ModerationOutcome.ModerationComments);
            Assert.AreEqual(actualViewModel.OnRegister,onRegister);
        }

        [Test]
        public async Task GetAppealDetails_returns_viewmodel()
        {
            var expectedApplicationDetails = GetApplication();
            var expectedOversightReview = GetOversightReview();
            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId)).ReturnsAsync(expectedApplicationDetails);
            _apiClient.Setup(x => x.GetOversightReview(_applicationId)).ReturnsAsync(() => expectedOversightReview);
            _apiClient.Setup(x => x.GetAppealDetails(_applicationId)).ReturnsAsync(_appealDetails);
            _roatpRegisterClient
                .Setup(x => x.GetOrganisationRegisterStatus(It.IsAny<GetOrganisationRegisterStatusRequest>()))
                .ReturnsAsync(new OrganisationRegisterStatus { UkprnOnRegister = onRegister });

            var actualViewModel = await _orchestrator.GetAppealDetailsViewModel(_applicationId, null);

            Assert.AreEqual(expectedApplicationDetails.ApplicationId, _applicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationId, actualViewModel.ApplicationSummary.ApplicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationReferenceNumber,
                actualViewModel.ApplicationSummary.ApplicationReferenceNumber);
            Assert.AreEqual(expectedApplicationDetails.ApplicationSubmittedDate,
                actualViewModel.ApplicationSummary.ApplicationSubmittedDate);
            Assert.AreEqual(expectedApplicationDetails.OrganisationName,
                actualViewModel.ApplicationSummary.OrganisationName);
            Assert.AreEqual(expectedApplicationDetails.ProviderRoute, actualViewModel.ApplicationSummary.ProviderRoute);
            Assert.AreEqual(expectedApplicationDetails.Ukprn, actualViewModel.ApplicationSummary.Ukprn);
            Assert.AreEqual(expectedOversightReview.Status, actualViewModel.OversightStatus);
            Assert.AreEqual(expectedApplicationDetails.ApplicationStatus,
                actualViewModel.ApplicationSummary.ApplicationStatus);
            Assert.AreEqual(expectedApplicationDetails.ApplicationEmailAddress,
                actualViewModel.ApplicationSummary.ApplicationEmailAddress);
            Assert.AreEqual(expectedApplicationDetails.AssessorReviewStatus,
                actualViewModel.ApplicationSummary.AssessorReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.GatewayReviewStatus,
                actualViewModel.GatewayOutcome.GatewayReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeDate,
                actualViewModel.GatewayOutcome.GatewayOutcomeMadeDate);
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeBy,
                actualViewModel.GatewayOutcome.GatewayOutcomeMadeBy);
            Assert.AreEqual(expectedApplicationDetails.GatewayComments, actualViewModel.GatewayOutcome.GatewayComments);
            Assert.AreEqual(expectedApplicationDetails.FinancialReviewStatus,
                actualViewModel.FinancialHealthOutcome.FinancialReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.FinancialGradeAwarded,
                actualViewModel.FinancialHealthOutcome.FinancialGradeAwarded);
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedOn,
                actualViewModel.FinancialHealthOutcome.FinancialHealthAssessedOn);
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedBy,
                actualViewModel.FinancialHealthOutcome.FinancialHealthAssessedBy);
            Assert.AreEqual(expectedApplicationDetails.ModerationReviewStatus,
                actualViewModel.ModerationOutcome.ModerationReviewStatus);
            Assert.AreEqual(expectedApplicationDetails.ModerationOutcomeMadeOn,
                actualViewModel.ModerationOutcome.ModerationOutcomeMadeOn);
            Assert.AreEqual(expectedApplicationDetails.ModeratedBy, actualViewModel.ModerationOutcome.ModeratedBy);
            Assert.AreEqual(expectedApplicationDetails.ModerationComments,
                actualViewModel.ModerationOutcome.ModerationComments);
            Assert.AreEqual(actualViewModel.Appeal,_appealDetails);
            Assert.AreEqual(actualViewModel.OnRegister,onRegister);
        }    
        
        [Test]
        public async Task Orchestrator_SaveOutcomePostRequestToCache_stores_expected_values()
        {
            var request = new OutcomePostRequest();
            await _orchestrator.SaveOutcomePostRequestToCache(request);
            _cacheStorageService.Verify(x => x.SaveToCache(It.IsAny<string>(), request, 1));
        }

        [Test]
        public async Task Orchestrator_uses_cache_data_when_provided_with_cache_key()
        {
            var autoFixture = new Fixture();

            var cacheKey = Guid.NewGuid();

            var cachedItem = autoFixture.Create<OutcomePostRequest>();
            cachedItem.ApplicationId = _applicationId;

            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId)).ReturnsAsync(GetApplication);

            _cacheStorageService.Setup(x =>
                    x.RetrieveFromCache<OutcomePostRequest>(It.Is<string>(key => key == cacheKey.ToString())))
                .ReturnsAsync(cachedItem);
            _roatpRegisterClient
                .Setup(x => x.GetOrganisationRegisterStatus(It.IsAny<GetOrganisationRegisterStatusRequest>()))
                .ReturnsAsync(new OrganisationRegisterStatus());

            var actualViewModel = await _orchestrator.GetOversightDetailsViewModel(_applicationId, cacheKey);

            Assert.AreEqual(cachedItem.ApproveGateway, actualViewModel.ApproveGateway);
            Assert.AreEqual(cachedItem.OversightStatus, actualViewModel.OversightStatus);
            Assert.AreEqual(cachedItem.ApproveModeration, actualViewModel.ApproveModeration);
            Assert.AreEqual(cachedItem.SuccessfulText, actualViewModel.SuccessfulText);
            Assert.AreEqual(cachedItem.SuccessfulAlreadyActiveText, actualViewModel.SuccessfulAlreadyActiveText);
            Assert.AreEqual(cachedItem.SuccessfulFitnessForFundingText, actualViewModel.SuccessfulFitnessForFundingText);
            Assert.AreEqual(cachedItem.UnsuccessfulText, actualViewModel.UnsuccessfulText);
            Assert.AreEqual(cachedItem.InProgressInternalText, actualViewModel.InProgressInternalText);
            Assert.AreEqual(cachedItem.InProgressExternalText, actualViewModel.InProgressExternalText);
        }


        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]

        public async Task OutcomeViewModel_assessment_outcome_tests(string financialReviewStatus, string financialGradeAwarded, string gatewayReviewStatus, string moderationReviewStatus, string assessmentOutcome)
        {
            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId))
                .ReturnsAsync(() => new ApplicationDetails
                {
                    FinancialReviewStatus = financialReviewStatus,
                    FinancialGradeAwarded = financialGradeAwarded,
                    GatewayReviewStatus = gatewayReviewStatus,
                    ModerationReviewStatus = moderationReviewStatus
                });

            var result = await _orchestrator.GetOversightDetailsViewModel(_applicationId, null);

            Assert.AreEqual(assessmentOutcome, result.ApplicationSummary.AssessmentOutcome);
        }

        [TestCase(GatewayReviewStatus.Pass, true, PassFailStatus.Passed)]
        [TestCase(GatewayReviewStatus.Pass, false, PassFailStatus.Failed)]
        [TestCase(GatewayReviewStatus.Fail, true, PassFailStatus.Failed)]
        [TestCase(GatewayReviewStatus.Fail, false, PassFailStatus.Passed)]
        [TestCase(GatewayReviewStatus.Fail, null, PassFailStatus.None)]
        [TestCase(GatewayReviewStatus.Pass, null, PassFailStatus.None)]
        [TestCase(GatewayReviewStatus.Removed, null, PassFailStatus.None)]
        public async Task TestGatewayGovernanceOutcomeIsCorrect(string gatewayReviewStatus, bool? approved, PassFailStatus expectedOutcome)
        {
            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId))
                .ReturnsAsync(() => new ApplicationDetails
                {
                    GatewayReviewStatus = gatewayReviewStatus
                });

            _apiClient.Setup(x => x.GetOversightReview(_applicationId))
                .ReturnsAsync(() => new GetOversightReviewResponse
                {
                    GatewayApproved = approved
                });

            var result = await _orchestrator.GetOversightDetailsViewModel(_applicationId, null);

            Assert.AreEqual(expectedOutcome, result.GatewayOutcome.GovernanceOutcome);
        }


        [TestCase(ModerationReviewStatus.Pass, true, PassFailStatus.Passed)]
        [TestCase(ModerationReviewStatus.Pass, false, PassFailStatus.Failed)]
        [TestCase(ModerationReviewStatus.Fail, true, PassFailStatus.Failed)]
        [TestCase(ModerationReviewStatus.Fail, false, PassFailStatus.Passed)]
        [TestCase(ModerationReviewStatus.Fail, null, PassFailStatus.None)]
        [TestCase(ModerationReviewStatus.Pass, null, PassFailStatus.None)]
        public async Task TestModerationGovernanceOutcomeIsCorrect(string moderationReviewStatus, bool? approved, PassFailStatus expectedOutcome)
        {
            _apiClient.Setup(x => x.GetApplicationDetails(_applicationId))
                .ReturnsAsync(() => new ApplicationDetails
                {
                    ModerationReviewStatus = moderationReviewStatus
                });

            _apiClient.Setup(x => x.GetOversightReview(_applicationId))
                .ReturnsAsync(() => new GetOversightReviewResponse
                {
                    ModerationApproved = approved
                });

            var result = await _orchestrator.GetOversightDetailsViewModel(_applicationId, null);

            Assert.AreEqual(expectedOutcome, result.ModerationOutcome.GovernanceOutcome);
        }


        private ApplicationDetails GetApplication()
        {
            return new ApplicationDetails
            {
                ApplicationId = _applicationId,
                OrganisationName = "ZZZ Limited",
                Ukprn = "123456768",
                ProviderRoute = "Main",
                ApplicationReferenceNumber = "APR000175",
                ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                ApplicationEmailAddress = "test@test.com",
                AssessorReviewStatus = "Pass",
                GatewayReviewStatus = GatewayReviewStatus.Pass,
                GatewayOutcomeMadeDate = DateTime.Today,
                GatewayOutcomeMadeBy = "joe",
                GatewayComments = "gateway commments",
                FinancialReviewStatus = FinancialReviewStatus.Pass,
                FinancialGradeAwarded = "Outstanding",
                FinancialHealthAssessedOn = DateTime.Today,
                FinancialHealthAssessedBy = "josephine",
                ModerationReviewStatus = ModerationReviewStatus.Pass,
                ModerationOutcomeMadeOn = DateTime.Today,
                ModeratedBy = "Lesley",
                ModerationComments = "moderation comments",
            };
        }

        private GetOversightReviewResponse GetOversightReview()
        {
            return new GetOversightReviewResponse
            {
                Id = _oversightReviewId,
            };
        }

        private  PendingOversightReviews GetApplicationsPending()
        {
            return new PendingOversightReviews
            {
                Reviews = new List<PendingOversightReview> {
                new PendingOversightReview
                {
                    ApplicationId = _applicationId,
                    OrganisationName = "ZZZ Limited",
                    Ukprn = "123456768",
                    ProviderRoute = "Main",
                    ApplicationReferenceNumber = "APR000175",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                },
                new PendingOversightReview
                {
                    ApplicationId = new Guid("a0fb2cdc-edf1-457c-96d7-2dc69cd5d8e8"),
                    OrganisationName = "AAA Limited",
                    Ukprn = "223456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000179",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20)
                },
                new PendingOversightReview
                {
                    ApplicationId = new Guid("cb84760b-931b-4724-a7fc-81e68659da10"),
                    OrganisationName = "BBB BBB Limited",
                    Ukprn = "523456765",
                    ProviderRoute = "Supporting",
                    ApplicationReferenceNumber = "APR000173",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                }
            }};
        }

        private static CompletedOversightReviews GetApplicationsDone()
        {
            return new CompletedOversightReviews
            {
                Reviews = new List<CompletedOversightReview> {
                new CompletedOversightReview
                {
                    OrganisationName = "FFF Limited",
                    Ukprn = "443456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000132",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 01),
                    OversightStatus = OversightReviewStatus.Successful,
                    ApplicationStatus = ApplicationStatus.Successful
                },
                new CompletedOversightReview
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 29),
                    OversightStatus = OversightReviewStatus.Unsuccessful,
                    ApplicationStatus = ApplicationStatus.Unsuccessful
                }}
            };
        }

        private PendingAppealOutcomes GetPendingAppealApplications()
        {
            return new PendingAppealOutcomes
            {
                Reviews = new List<PendingAppealOutcome> {
                new PendingAppealOutcome
                {
                    ApplicationId = _applicationId,
                    OrganisationName = "ZZZ Limited",
                    Ukprn = "123456768",
                    ProviderRoute = "Main",
                    ApplicationReferenceNumber = "APR000175",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                },
                new PendingAppealOutcome
                {
                    ApplicationId = new Guid("a0fb2cdc-edf1-457c-96d7-2dc69cd5d8e8"),
                    OrganisationName = "AAA Limited",
                    Ukprn = "223456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000179",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20)
                },
                new PendingAppealOutcome
                {
                    ApplicationId = new Guid("cb84760b-931b-4724-a7fc-81e68659da10"),
                    OrganisationName = "BBB BBB Limited",
                    Ukprn = "523456765",
                    ProviderRoute = "Supporting",
                    ApplicationReferenceNumber = "APR000173",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                }
            }
            };
        }

        private static CompletedAppealOutcomes GetCompletedApplicationsDone()
        {
            return new CompletedAppealOutcomes
            {
                Reviews = new List<CompletedAppealOutcome> {
                new CompletedAppealOutcome
                {
                    OrganisationName = "FFF Limited",
                    Ukprn = "443456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000132",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                    AppealDeterminedDate = new DateTime(2019, 10, 01),
                    OversightStatus = OversightReviewStatus.Successful,
                    AppealStatus = AppealStatus.Successful
                },
                new CompletedAppealOutcome
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    AppealDeterminedDate = new DateTime(2019, 10, 29),
                    OversightStatus = OversightReviewStatus.Unsuccessful,
                    AppealStatus = AppealStatus.Unsuccessful
                }}
            };
        }
    }
}

