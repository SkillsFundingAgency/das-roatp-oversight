using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Models.Partials;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightControllerTests
    {
        private Mock<ISearchTermValidator> _searchTermValidator;
        private Mock<IOversightOrchestrator> _oversightOrchestrator;
        private Mock<IApplyApiClient> _applyApiClient;
        private Mock<IApplicationOutcomeOrchestrator> _outcomeOrchestrator;
        private ITempDataDictionary _tempDataDictionary;

        private static readonly Fixture _autoFixture = new Fixture();
        private OversightController _controller;
        private readonly Guid _applicationDetailsApplicationId = Guid.NewGuid();
        private const string _ukprnOfCompletedOversightApplication = "11112222";

        [SetUp]
        public void SetUp()
        {
            _searchTermValidator = new Mock<ISearchTermValidator>();
            _oversightOrchestrator = new Mock<IOversightOrchestrator>();
            _outcomeOrchestrator = new Mock<IApplicationOutcomeOrchestrator>();
            _applyApiClient=new Mock<IApplyApiClient>();

            _controller = new OversightController(_searchTermValidator.Object,
                                                  _outcomeOrchestrator.Object,
                                                  _oversightOrchestrator.Object,
                                                  _applyApiClient.Object)
            {
                ControllerContext = MockedControllerContext.Setup()
            };

            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            _tempDataDictionary = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            _controller.TempData = _tempDataDictionary;
        }

        [Test]
        public async Task GetApplications_returns_view_with_expected_viewmodel()
        {
            var applicationsPending = new PendingOversightReviews
            {
                Reviews = new List<PendingOversightReview> { new PendingOversightReview {  ApplicationId = _applicationDetailsApplicationId} }
            };

            var applicationsDone = new CompletedOversightReviews
            {
                Reviews = new List<CompletedOversightReview> { new CompletedOversightReview { Ukprn = _ukprnOfCompletedOversightApplication} }
            };

            var PendingAppealapplications = new PendingAppealOutcomes
            {
                Reviews = new List<PendingAppealOutcome> { new PendingAppealOutcome { ApplicationId = _applicationDetailsApplicationId } }
            };

            var CompletedAppealapplications = new CompletedAppealOutcomes
            {
                Reviews = new List<CompletedAppealOutcome> { new CompletedAppealOutcome { Ukprn = _ukprnOfCompletedOversightApplication } }
            };

            var viewModel = new ApplicationsViewModel {ApplicationDetails = applicationsPending,ApplicationCount = 1, OverallOutcomeDetails = applicationsDone, OverallOutcomeCount = 1,PendingAppealsDetails=PendingAppealapplications,AppealsCount=1,CompleteAppealsDetails=CompletedAppealapplications,AppealsOutcomeCount=1};

            _oversightOrchestrator.Setup(x => x.GetApplicationsViewModel(null,null,null,null)).ReturnsAsync(viewModel);

            var result = await _controller.Applications(null,null,null,null) as ViewResult;
            var actualViewModel = result?.Model as ApplicationsViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationDetails.Reviews.FirstOrDefault().ApplicationId);
            Assert.AreEqual(_ukprnOfCompletedOversightApplication, actualViewModel.OverallOutcomeDetails.Reviews.FirstOrDefault().Ukprn);
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.PendingAppealsDetails.Reviews.FirstOrDefault().ApplicationId);
            Assert.AreEqual(_ukprnOfCompletedOversightApplication, actualViewModel.CompleteAppealsDetails.Reviews.FirstOrDefault().Ukprn);
        }

        [Test]
        public async Task GetOutcome_returns_view_with_expected_viewModel()
        {
            var viewModel = new OutcomeDetailsViewModel { ApplicationSummary = new ApplicationSummaryViewModel{ ApplicationId = _applicationDetailsApplicationId} };
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var request = new OutcomeRequest {ApplicationId = _applicationDetailsApplicationId};
            var result = await _controller.Outcome(request) as ViewResult;
            var actualViewModel = result?.Model as OutcomeDetailsViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationSummary.ApplicationId);
        }



        [Test]
        public async Task GetAppeal_returns_view_with_expected_viewModel()
        {
            var viewModel = new AppealViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };
            _oversightOrchestrator.Setup(x => x.GetAppealDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var request = new AppealRequest() { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.Appeal(request) as ViewResult;
            var actualViewModel = result?.Model as AppealViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationSummary.ApplicationId);
        }

        [Test]
        public async Task GetAppealOutcome_returns_view_with_expected_viewModel()
        {
            var viewModel = new AppealViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };
            _oversightOrchestrator.Setup(x => x.GetAppealDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var request = new AppealRequest() { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.AppealOutcome(request) as ViewResult;
            var actualViewModel = result?.Model as AppealViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationSummary.ApplicationId);
        }

        [TestCase(OversightReviewStatus.Successful)]
        [TestCase(OversightReviewStatus.Unsuccessful)]
        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding)]
        public async Task GetOutcome_returns_applications_view_when_oversight_status_is_successful_or_unsuccessful(OversightReviewStatus status)
        {
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).Throws<InvalidStateException>();

            var request = new OutcomeRequest { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.Outcome(request) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Applications"));
        }

        [TestCase(AppealStatus.Successful)]
        [TestCase(AppealStatus.Unsuccessful)]
        [TestCase(AppealStatus.SuccessfulAlreadyActive)]
        [TestCase(AppealStatus.SuccessfulFitnessForFunding)]
        public async Task GetAppeal_returns_applications_view_when_appeal_status_is_successful_or_unsuccessful(string status)
        {
            _oversightOrchestrator.Setup(x => x.GetAppealDetailsViewModel(_applicationDetailsApplicationId, null)).Throws<InvalidStateException>();

            var request = new AppealRequest() { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.Appeal(request) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Applications"));
        }

        [TestCase(AppealStatus.Successful)]
        [TestCase(AppealStatus.Unsuccessful)]
        [TestCase(AppealStatus.SuccessfulAlreadyActive)]
        [TestCase(AppealStatus.SuccessfulFitnessForFunding)]
        public async Task GetAppealOutcome_returns_applications_view_when_appealoutcome_status_is_successful_or_unsuccessful(string status)
        {
            _oversightOrchestrator.Setup(x => x.GetAppealDetailsViewModel(_applicationDetailsApplicationId, null)).Throws<InvalidStateException>();

            var request = new AppealRequest() { ApplicationId = _applicationDetailsApplicationId };
            var result = await _controller.AppealOutcome(request) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Applications"));
        }

        [Test]
        public async Task Post_Outcome_redirects_to_confirmation()
        {
            var viewModel = new OutcomeDetailsViewModel { ApplicationSummary = new ApplicationSummaryViewModel{ ApplicationId = _applicationDetailsApplicationId }};

            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var command = new OutcomePostRequest
            {
                ApplicationId = _applicationDetailsApplicationId,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = GatewayReviewStatus.Fail,
                ApproveModeration = ModerationReviewStatus.Fail,
                UnsuccessfulText =  "test",
                IsGatewayFail = false
            };

            var result = await _controller.Outcome(command) as RedirectToActionResult;
            Assert.AreEqual("ConfirmOutcome", result.ActionName);
        }

        [Test]
        public async Task Post_Appeal_redirects_to_confirmation()
        {
            var viewModel = new AppealViewModel() { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

            _oversightOrchestrator.Setup(x => x.GetAppealDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);
                
            var command = new AppealPostRequest()
            {
                ApplicationId = _applicationDetailsApplicationId,
                AppealStatus = AppealStatus.Unsuccessful,
                UnsuccessfulText = "test"
            };

            var result = await _controller.Appeal(command) as RedirectToActionResult;
            Assert.AreEqual("ConfirmAppealOutcome", result.ActionName);
        }


        [Test]
        public async Task Post_Outcome_Gateway_Fail_Records_Gateway_Fail_Outcome()
        {
            var viewModel = new OutcomeDetailsViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var command = new OutcomePostRequest
            {
                ApplicationId = _applicationDetailsApplicationId,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = GatewayReviewStatus.Fail,
                ApproveModeration = ModerationReviewStatus.Fail,
                UnsuccessfulText = "test",
                IsGatewayFail = true
            };

            await _controller.Outcome(command);

            _outcomeOrchestrator.Verify(x => x.RecordGatewayFailOutcome(It.Is<Guid>(id => id == command.ApplicationId), It.IsAny<string>(), It.IsAny<string>()));
        }


        [Test]
        public async Task Post_Outcome_Gateway_Removed_Records_Gateway_Removed_Outcome()
        {
            var viewModel = new OutcomeDetailsViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var command = new OutcomePostRequest
            {
                ApplicationId = _applicationDetailsApplicationId,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = GatewayReviewStatus.Fail,
                ApproveModeration = ModerationReviewStatus.Fail,
                UnsuccessfulText = "test",
                IsGatewayRemoved = true
            };

            await _controller.Outcome(command);

            _outcomeOrchestrator.Verify(x => x.RecordGatewayRemovedOutcome(It.Is<Guid>(id => id == command.ApplicationId), It.IsAny<string>(), It.IsAny<string>()));
        }


        [Test]
        public async Task Post_Outcome_Gateway_Fail_Redirects_To_Confirmed_Page()
        {
            var viewModel = new OutcomeDetailsViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var command = new OutcomePostRequest
            {
                ApplicationId = _applicationDetailsApplicationId,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = GatewayReviewStatus.Fail,
                ApproveModeration = ModerationReviewStatus.Fail,
                UnsuccessfulText = "test",
                IsGatewayFail = true
            };

            var result = await _controller.Outcome(command) as RedirectToActionResult;
            Assert.AreEqual("Confirmed", result.ActionName);
        }

        private static IFormFile GenerateFile()
        {
            var fileName = "test.pdf";
            var content = _autoFixture.Create<string>();
            return new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(content)),
                0,
                content.Length,
                fileName,
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }
    }
}
