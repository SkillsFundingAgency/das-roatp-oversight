﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightOrchestratorTests
    {
        private OversightOrchestrator _orchestrator;
        private Mock<IRoatpApplicationApiClient> _apiClient;

        [SetUp]
        public void SetUp()
        {
            _apiClient = new Mock<IRoatpApplicationApiClient>();
            _orchestrator = new OversightOrchestrator(_apiClient.Object, Mock.Of<ILogger<OversightOrchestrator>>());
        }

        [Test]
        public async Task Orchestrator_builds_view_model_from_api()
        {
            var expectedApplicationsPending = GetApplicationsPending();
            _apiClient.Setup(x => x.GetOversightsPending()).ReturnsAsync(expectedApplicationsPending);
            _apiClient.Setup(x => x.GetOversightsCompleted()).ReturnsAsync(GetApplicationsDone());
            var actualViewModel = await _orchestrator.GetOversightOverviewViewModel();

            var expectedViewModel = new OverallOutcomeViewModel
            {
                ApplicationDetails = GetApplicationsPending(), OverallOutcomeDetails = GetApplicationsDone()
            };
            expectedViewModel.ApplicationCount = expectedViewModel.ApplicationDetails.Count;
            expectedViewModel.OverallOutcomeCount = expectedViewModel.OverallOutcomeDetails.Count;



            Assert.AreEqual(actualViewModel.ApplicationDetails.Count, expectedViewModel.ApplicationDetails.Count);


            //Assert.That(viewModel.OverallOutcomeDetails, Is.SameAs(GetApplicationsDone()));
           // Assert.AreEqual(GetApplicationsPending().Count,viewModel.ApplicationCount);
           // Assert.AreEqual(GetApplicationsDone().Count, viewModel.OverallOutcomeCount);
        }



        private static List<ApplicationDetails> GetApplicationsPending()
        {
            return new List<ApplicationDetails>
            {
                new ApplicationDetails
                {
                    ApplicationId = new Guid("2e8ffe21-f622-4eef-af93-22e0ad0c6737"),
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
                    OversightStatus = "SUCCESSFUL"
                },
                new OverallOutcomeDetails
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 29),
                    OversightStatus = "UNSUCCESSFUL"
                }
            };
        }
    }
}

