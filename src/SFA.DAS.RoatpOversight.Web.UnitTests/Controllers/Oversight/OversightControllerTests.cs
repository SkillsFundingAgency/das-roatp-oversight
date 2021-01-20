using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightControllerTests
    {
        private Mock<IOversightOrchestrator> _oversightOrchestrator;
        private Mock<IApplicationOutcomeOrchestrator> _outcomeOrchestrator;

        private OversightController _controller;
        private readonly Guid _applicationDetailsApplicationId = Guid.NewGuid();
        private const string _ukprnOfCompletedOversightApplication = "11112222";

        [SetUp]
        public void SetUp()
        {
            _oversightOrchestrator = new Mock<IOversightOrchestrator>();
            _outcomeOrchestrator = new Mock<IApplicationOutcomeOrchestrator>();

            _controller = new OversightController(_outcomeOrchestrator.Object,
                                                  _oversightOrchestrator.Object)
            {
                ControllerContext = MockedControllerContext.Setup()
            };
        }

        [Test]
        public async Task GetApplications_returns_view_with_expected_viewmodel()
        {
            var applicationsPending = new List<ApplicationDetails>
            {
                new ApplicationDetails {ApplicationId = _applicationDetailsApplicationId}
            };

            var applicationsDone = new List<OverallOutcomeDetails>
            {
                new OverallOutcomeDetails {Ukprn = _ukprnOfCompletedOversightApplication}
            };

            var viewModel = new ApplicationsViewModel {ApplicationDetails = applicationsPending,ApplicationCount = 1, OverallOutcomeDetails = applicationsDone, OverallOutcomeCount = 1};

            _oversightOrchestrator.Setup(x => x.GetOversightOverviewViewModel()).ReturnsAsync(viewModel);

            var result = await _controller.Applications() as ViewResult;
            var actualViewModel = result?.Model as ApplicationsViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationDetails.FirstOrDefault().ApplicationId);
            Assert.AreEqual(_ukprnOfCompletedOversightApplication, actualViewModel.OverallOutcomeDetails.FirstOrDefault().Ukprn);
        }

        [Test]
        public async Task GetOutcome_returns_view_with_expected_viewModel()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var request = new OutcomeRequest {ApplicationId = _applicationDetailsApplicationId};
            var result = await _controller.Outcome(request) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }

        [TestCase(OversightReviewStatus.Successful)]
        [TestCase(OversightReviewStatus.Unsuccessful)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding)]
        public async Task GetOutcome_returns_applications_view_when_oversight_status_is_successful_or_unsuccessful(string status)
        {
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).Throws<InvalidStateException>();

            var request = new OutcomeRequest { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.Outcome(request) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Applications"));
        }

        [Test]
        public async Task Post_Outcome_redirects_to_confirmation()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };

            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var command = new OutcomePostRequest
            {
                ApplicationId = _applicationDetailsApplicationId,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = GatewayReviewStatus.Fail,
                ApproveModeration = ModerationReviewStatus.Fail,
                UnsuccessfulText =  "test"
            };

            var result = await _controller.Outcome(command) as RedirectToActionResult;
            Assert.AreEqual("ConfirmOutcome", result.ActionName);
        }
    }
}
