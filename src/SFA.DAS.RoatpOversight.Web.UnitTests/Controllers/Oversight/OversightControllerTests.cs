using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Models.Partials;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightControllerTests
    {
        private Mock<IOversightOrchestrator> _oversightOrchestrator;
        private Mock<IApplicationOutcomeOrchestrator> _outcomeOrchestrator;
        private Mock<IAppealOrchestrator> _appealOrchestrator;

        private OversightController _controller;
        private readonly Guid _applicationDetailsApplicationId = Guid.NewGuid();
        private const string _ukprnOfCompletedOversightApplication = "11112222";

        [SetUp]
        public void SetUp()
        {
            _oversightOrchestrator = new Mock<IOversightOrchestrator>();
            _outcomeOrchestrator = new Mock<IApplicationOutcomeOrchestrator>();
            _appealOrchestrator = new Mock<IAppealOrchestrator>();

            _controller = new OversightController(_outcomeOrchestrator.Object,
                                                  _oversightOrchestrator.Object,
                                                  _appealOrchestrator.Object)
            {
                ControllerContext = MockedControllerContext.Setup()
            };
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

            var viewModel = new ApplicationsViewModel {ApplicationDetails = applicationsPending,ApplicationCount = 1, OverallOutcomeDetails = applicationsDone, OverallOutcomeCount = 1};

            _oversightOrchestrator.Setup(x => x.GetApplicationsViewModel()).ReturnsAsync(viewModel);

            var result = await _controller.Applications() as ViewResult;
            var actualViewModel = result?.Model as ApplicationsViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationDetails.Reviews.FirstOrDefault().ApplicationId);
            Assert.AreEqual(_ukprnOfCompletedOversightApplication, actualViewModel.OverallOutcomeDetails.Reviews.FirstOrDefault().Ukprn);
        }

        [Test]
        public async Task GetOutcome_returns_view_with_expected_viewModel()
        {
            var viewModel = new OutcomeViewModel { ApplicationSummary = new ApplicationSummaryViewModel{ ApplicationId = _applicationDetailsApplicationId} };
            _oversightOrchestrator.Setup(x => x.GetOversightDetailsViewModel(_applicationDetailsApplicationId, null)).ReturnsAsync(viewModel);

            var request = new OutcomeRequest {ApplicationId = _applicationDetailsApplicationId};
            var result = await _controller.Outcome(request) as ViewResult;
            var actualViewModel = result?.Model as OutcomeViewModel;

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

        [Test]
        public async Task Post_Outcome_redirects_to_confirmation()
        {
            var viewModel = new OutcomeViewModel { ApplicationSummary = new ApplicationSummaryViewModel{ ApplicationId = _applicationDetailsApplicationId }};

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
        public async Task Post_Outcome_Gateway_Fail_Records_Gateway_Fail_Outcome()
        {
            var viewModel = new OutcomeViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

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
            var viewModel = new OutcomeViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

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
            var viewModel = new OutcomeViewModel { ApplicationSummary = new ApplicationSummaryViewModel { ApplicationId = _applicationDetailsApplicationId } };

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

        [Test]
        public async Task Post_Appeal_File_Upload_Is_Recorded()
        {
            var applicationId = Guid.NewGuid();

            _appealOrchestrator.Setup(x =>
                x.UploadAppealFile(It.IsAny<Guid>(), It.IsAny<FileUpload>(), It.IsAny<string>(), It.IsAny<string>()));

            var file = GenerateMockFile();

            var request = new AppealPostRequest
            {
                ApplicationId = applicationId,
                FileUpload = file.Object,
                SelectedOption = AppealPostRequest.SubmitOption.Upload
            };

            await _controller.Appeal(request);

            _appealOrchestrator.Verify(x => x.UploadAppealFile(It.Is<Guid>(id => id == applicationId),
                It.Is<FileUpload>(f => f.FileName == file.Object.FileName && f.ContentType == file.Object.ContentType),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        private Mock<IFormFile> GenerateMockFile()
        {
            var fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            return fileMock;
        }
    }
}
