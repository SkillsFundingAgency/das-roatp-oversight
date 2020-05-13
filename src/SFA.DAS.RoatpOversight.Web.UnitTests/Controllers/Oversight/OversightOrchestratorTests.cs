using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightOrchestratorTests
    {
        private OversightOrchestrator _orchestrator;
        private Mock<IApplyApiClient> _apiClient;
        private Mock<IWebConfiguration> _configuration;
        private string _dashboardAddress;
        private Guid _applicationId;
        [SetUp]
        public void SetUp()
        {
            _applicationId = Guid.NewGuid();
            _apiClient = new Mock<IApplyApiClient>();
            _configuration = new Mock<IWebConfiguration>();
             _dashboardAddress   = "https://dashboard";
            _orchestrator = new OversightOrchestrator(_apiClient.Object,  Mock.Of<ILogger<OversightOrchestrator>>());
        }

        [Test]
        public async Task Orchestrator_builds_view_model_from_api()
        {
            var expectedApplicationsPending = GetApplicationsPending();
            _apiClient.Setup(x => x.GetOversightsPending()).ReturnsAsync(expectedApplicationsPending);
            _apiClient.Setup(x => x.GetOversightsCompleted()).ReturnsAsync(GetApplicationsDone());
            string dashboardAddress;
            _configuration.Setup(x => x.EsfaAdminServicesBaseUrl).Returns(_dashboardAddress);
            var actualViewModel = await _orchestrator.GetOversightOverviewViewModel();

            var expectedViewModel = new OverallOutcomeViewModel
            {
                ApplicationDetails = GetApplicationsPending(), OverallOutcomeDetails = GetApplicationsDone()
            };
            expectedViewModel.ApplicationCount = expectedViewModel.ApplicationDetails.Count;
            expectedViewModel.OverallOutcomeCount = expectedViewModel.OverallOutcomeDetails.Count;

            Assert.AreEqual(actualViewModel.ApplicationCount, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeCount, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.Count, expectedViewModel.ApplicationCount);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.Count, expectedViewModel.OverallOutcomeCount);
            Assert.AreEqual(actualViewModel.ApplicationDetails.First().ApplicationId, expectedViewModel.ApplicationDetails.First().ApplicationId);
            Assert.AreEqual(actualViewModel.OverallOutcomeDetails.First().Ukprn, expectedViewModel.OverallOutcomeDetails.First().Ukprn);
        }

        [Test]
        public async Task Orchestrator_builds_view_model_details_from_api()
        {
            var expectedApplicationDetails = GetApplicationsPending().First();
            _apiClient.Setup(x => x.GetOversightDetails(_applicationId)).ReturnsAsync(expectedApplicationDetails);
              var actualViewModel = await _orchestrator.GetOversightDetailsViewModel(_applicationId);

            Assert.AreEqual(expectedApplicationDetails.ApplicationId, _applicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationId, actualViewModel.ApplicationId);
            Assert.AreEqual(expectedApplicationDetails.ApplicationReferenceNumber, actualViewModel.ApplicationReferenceNumber);
            Assert.AreEqual(expectedApplicationDetails.ApplicationSubmittedDate, actualViewModel.ApplicationSubmittedDate);
            Assert.AreEqual(expectedApplicationDetails.OrganisationName, actualViewModel.OrganisationName);
            Assert.AreEqual(expectedApplicationDetails.ProviderRoute, actualViewModel.ProviderRoute);
            Assert.AreEqual(expectedApplicationDetails.Ukprn, actualViewModel.Ukprn);
        }


        private  List<ApplicationDetails> GetApplicationsPending()
        {
            return new List<ApplicationDetails>
            {
                new ApplicationDetails
                {
                    ApplicationId = _applicationId,
                    OrganisationName = "ZZZ Limited",
                    Ukprn = "123456768",
                    ProviderRoute = "Main",
                    ApplicationReferenceNumber = "APR000175",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                },
                new ApplicationDetails
                {
                    ApplicationId = new Guid("a0fb2cdc-edf1-457c-96d7-2dc69cd5d8e8"),
                    OrganisationName = "AAA Limited",
                    Ukprn = "223456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000179",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20)
                },
                new ApplicationDetails
                {
                    ApplicationId = new Guid("cb84760b-931b-4724-a7fc-81e68659da10"),
                    OrganisationName = "BBB BBB Limited",
                    Ukprn = "523456765",
                    ProviderRoute = "Supporting",
                    ApplicationReferenceNumber = "APR000173",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                }
            };
        }


        private static List<OverallOutcomeDetails> GetApplicationsDone()
        {
            return new List<OverallOutcomeDetails>
            {
                new OverallOutcomeDetails
                {
                    OrganisationName = "FFF Limited",
                    Ukprn = "443456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000132",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 01),
                    OversightStatus = OversightReviewStatus.Successful
                },
                new OverallOutcomeDetails
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 29),
                    OversightStatus = OversightReviewStatus.Unsuccessful
                }
            };
        }
    }
}

