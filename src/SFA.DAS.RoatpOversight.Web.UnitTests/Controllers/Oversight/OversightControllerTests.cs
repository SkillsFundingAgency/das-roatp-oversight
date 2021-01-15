using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.ViewModels;

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

            var viewModel = new OverallOutcomeViewModel {ApplicationDetails = applicationsPending,ApplicationCount = 1, OverallOutcomeDetails = applicationsDone, OverallOutcomeCount = 1};

            _oversightOrchestrator.Setup(x => x.GetOversightOverviewViewModel()).ReturnsAsync(viewModel);

            var result = await _controller.Applications() as ViewResult;
            var actualViewModel = result?.Model as OverallOutcomeViewModel;

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
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.Outcome(_applicationDetailsApplicationId) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }


        [TestCase(OversightReviewStatus.Successful)]
        [TestCase(OversightReviewStatus.Unsuccessful)]
        public async Task GetOutcome_returns_applications_view_when_oversight_status_is_successful_or_unsuccessful(string status)
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId, OversightStatus = status};
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.Outcome(_applicationDetailsApplicationId) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Applications"));
        }

        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail)]
        public async Task EvaluateOutcome_posts_successful_status_returns_successful_view_as_expected(string oversightStatus, string gatewayStatus, string moderationStatus)
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            var expectedViewModel = new OutcomeSuccessStatusViewModel { ApplicationId = _applicationDetailsApplicationId, ApplicationSubmittedDate = DateTime.Today };
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.EvaluateOutcome(_applicationDetailsApplicationId, oversightStatus, gatewayStatus, moderationStatus) as ViewResult;
            var actualViewModel = result?.Model as OutcomeStatusViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            
            Assert.AreEqual(expectedViewModel.ApplicationId,actualViewModel.ApplicationId);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }

        [Test]
        public async Task EvaluateOutcome_posts_no_answer_returns_original_view_with_error_messages_as_expected()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            var status = string.Empty;
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.EvaluateOutcome(_applicationDetailsApplicationId, status, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);

            Assert.AreEqual(viewModel.ApplicationId, actualViewModel.ApplicationId);
            Assert.AreEqual(1,actualViewModel.ErrorMessages.Count);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }
    }
}
