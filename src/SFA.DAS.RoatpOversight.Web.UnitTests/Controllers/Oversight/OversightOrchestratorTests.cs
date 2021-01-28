using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Castle.DynamicProxy.Generators;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
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
        private string _dashboardAddress;
        private Guid _applicationId;
        [SetUp]
        public void SetUp()
        {
            _applicationId = Guid.NewGuid();
            _apiClient = new Mock<IApplyApiClient>();
            _configuration = new Mock<IWebConfiguration>();
            _cacheStorageService = new Mock<ICacheStorageService>();
             _dashboardAddress   = "https://dashboard";
            _orchestrator = new OversightOrchestrator(_apiClient.Object,  Mock.Of<ILogger<OversightOrchestrator>>(), _cacheStorageService.Object);
        }

        [Test]
        public async Task Orchestrator_builds_overview_viewmodel_from_api()
        {
            var expectedApplicationsPending = GetApplicationsPending();
            _apiClient.Setup(x => x.GetOversightsPending()).ReturnsAsync(expectedApplicationsPending);
            _apiClient.Setup(x => x.GetOversightsCompleted()).ReturnsAsync(GetApplicationsDone());
            _configuration.Setup(x => x.EsfaAdminServicesBaseUrl).Returns(_dashboardAddress);
            var actualViewModel = await _orchestrator.GetApplicationsViewModel();

            var expectedViewModel = new ApplicationsViewModel
            {
                ApplicationDetails = GetApplicationsPending(), OverallOutcomeDetails = GetApplicationsDone()
            };
            expectedViewModel.ApplicationCount = expectedViewModel.ApplicationDetails.Reviews.Count;
            expectedViewModel.OverallOutcomeCount = expectedViewModel.OverallOutcomeDetails.Reviews.Count;

            Assert.AreEqual(actualViewModel.ApplicationCount, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeCount, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.Reviews.Count, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.Reviews.Count, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.Reviews.First().ApplicationId, expectedViewModel.ApplicationDetails.Reviews.First().ApplicationId);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.Reviews.First().Ukprn, expectedViewModel.OverallOutcomeDetails.Reviews.First().Ukprn);
        }

        [Test]
        public async Task Orchestrator_builds_details_viewmodel_from_api()
        {
            var expectedApplicationDetails = GetApplication();
            _apiClient.Setup(x => x.GetOversightDetails(_applicationId)).ReturnsAsync(expectedApplicationDetails);
              var actualViewModel = await _orchestrator.GetOversightDetailsViewModel(_applicationId, null);

            Assert.AreEqual(expectedApplicationDetails.ApplicationId, _applicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationId, actualViewModel.ApplicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationReferenceNumber, actualViewModel.ApplicationReferenceNumber);
            Assert.AreEqual(expectedApplicationDetails.ApplicationSubmittedDate, actualViewModel.ApplicationSubmittedDate);
            Assert.AreEqual(expectedApplicationDetails.OrganisationName, actualViewModel.OrganisationName);
            Assert.AreEqual(expectedApplicationDetails.ProviderRoute, actualViewModel.ProviderRoute);
            Assert.AreEqual(expectedApplicationDetails.Ukprn, actualViewModel.Ukprn);
            Assert.AreEqual(expectedApplicationDetails.OversightStatus, actualViewModel.OversightStatus);
            Assert.AreEqual(expectedApplicationDetails.ApplicationStatus, actualViewModel.ApplicationStatus);
            //Assert.AreEqual("Failed", actualViewModel.AssessmentOutcome );
            Assert.AreEqual(expectedApplicationDetails.ApplicationEmailAddress, actualViewModel.ApplicationEmailAddress);
            Assert.AreEqual(expectedApplicationDetails.AssessorReviewStatus, actualViewModel.AssessorReviewStatus); 
            Assert.AreEqual(expectedApplicationDetails.GatewayReviewStatus, actualViewModel.GatewayReviewStatus); 
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeDate, actualViewModel.GatewayOutcomeMadeDate); 
            Assert.AreEqual(expectedApplicationDetails.GatewayOutcomeMadeBy, actualViewModel.GatewayOutcomeMadeBy); 
            Assert.AreEqual(expectedApplicationDetails.GatewayComments, actualViewModel.GatewayComments); 
            Assert.AreEqual(expectedApplicationDetails.FinancialReviewStatus, actualViewModel.FinancialReviewStatus); 
            Assert.AreEqual(expectedApplicationDetails.FinancialGradeAwarded, actualViewModel.FinancialGradeAwarded); 
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedOn, actualViewModel.FinancialHealthAssessedOn);
            Assert.AreEqual(expectedApplicationDetails.FinancialHealthAssessedBy, actualViewModel.FinancialHealthAssessedBy); 
            Assert.AreEqual(expectedApplicationDetails.ModerationReviewStatus, actualViewModel.ModerationReviewStatus); 
            Assert.AreEqual(expectedApplicationDetails.ModerationOutcomeMadeOn, actualViewModel.ModerationOutcomeMadeOn); 
            Assert.AreEqual(expectedApplicationDetails.ModeratedBy, actualViewModel.ModeratedBy);
            Assert.AreEqual(expectedApplicationDetails.ModerationComments, actualViewModel.ModerationComments);
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

            _apiClient.Setup(x => x.GetOversightDetails(_applicationId)).ReturnsAsync(GetApplication);

            _cacheStorageService.Setup(x =>
                    x.RetrieveFromCache<OutcomePostRequest>(It.Is<string>(key => key == cacheKey.ToString())))
                .ReturnsAsync(cachedItem);

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
                ModerationComments = "moderation comments"
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
                    ApplicationStatus = ApplicationStatus.Approved
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
                    ApplicationStatus = ApplicationStatus.Rejected
                }}
            };
        }
    }
}

