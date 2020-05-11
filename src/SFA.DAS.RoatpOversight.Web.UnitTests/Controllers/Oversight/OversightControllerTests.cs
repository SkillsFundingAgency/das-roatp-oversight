using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.UnitTests.MockedObjects;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightControllerTests
    {
        private Mock<Services.IOversightOrchestrator> _orchestrator;
        private OversightController _controller;
        private readonly Guid _applicationDetailsApplicationId = Guid.NewGuid();
        private string _ukprnOfCompletedOversightApplication = "11112222";

        [SetUp]
        public void SetUp()
        {
            _orchestrator = new Mock<Services.IOversightOrchestrator>();

            _controller = new OversightController(_orchestrator.Object, Mock.Of<ILogger<OversightController>>())
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

            _orchestrator.Setup(x => x.GetOversightOverviewViewModel()).ReturnsAsync(viewModel);

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
            _orchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.Outcome(_applicationDetailsApplicationId) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }


        [Test]
        public async Task EvaluateOutcome_posts_successful_answer_returns_successful_view_as_expected()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            var expectedViewModel = new OutcomeSuccessViewModel { ApplicationId = _applicationDetailsApplicationId, ApplicationSubmittedDate = DateTime.Today };
            var status = OversightReviewStatus.Successful;
            _orchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.EvaluateOutcome(_applicationDetailsApplicationId, status) as ViewResult;
            var actualViewModel = result?.Model as OutcomeSuccessViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            
            Assert.AreEqual(expectedViewModel.ApplicationId,actualViewModel.ApplicationId);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }


        [Test]
        public async Task EvaluateOutcome_posts_unsuccessful_answer_returns_unsuccessful_view_as_expected()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            var expectedViewModel = new OutcomeSuccessViewModel { ApplicationId = _applicationDetailsApplicationId, ApplicationSubmittedDate = DateTime.Today };
            var status = OversightReviewStatus.Unsuccessful;
            _orchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.EvaluateOutcome(_applicationDetailsApplicationId, status) as ViewResult;
            var actualViewModel = result?.Model as OutcomeSuccessViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);

            Assert.AreEqual(expectedViewModel.ApplicationId, actualViewModel.ApplicationId);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }



        [Test]
        public async Task EvaluateOutcome_posts_no_answer_returns_original_view_with_error_messages_as_expected()
        {
            var viewModel = new OutcomeViewModel { ApplicationId = _applicationDetailsApplicationId };
            var status = string.Empty;
            _orchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId)).ReturnsAsync(viewModel);

            var result = await _controller.EvaluateOutcome(_applicationDetailsApplicationId, status) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);

            Assert.AreEqual(viewModel.ApplicationId, actualViewModel.ApplicationId);
            Assert.AreEqual(1,actualViewModel.ErrorMessages.Count);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationId);
        }
    }
}
