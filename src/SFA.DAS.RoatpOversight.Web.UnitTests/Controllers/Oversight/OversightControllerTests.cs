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
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Controllers;
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
        private ITempDataDictionary _tempDataDictionary;

        private static readonly Fixture _autoFixture = new Fixture();
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
        public async Task Get_Appeal_Returns_View()
        {
            var request = new AppealRequest
            {
                ApplicationId =  Guid.NewGuid()
            };

            _appealOrchestrator.Setup(x =>
                    x.GetAppealViewModel(
                        It.Is<AppealRequest>(r => r.ApplicationId == request.ApplicationId),
                        It.IsAny<string>()))
                .ReturnsAsync(() => new AppealViewModel());

            var result = await _controller.Appeal(request);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Post_Appeal_File_Upload_Is_Recorded()
        {
            var applicationId = Guid.NewGuid();

            _appealOrchestrator.Setup(x =>
                x.UploadAppealFile(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()));

            var file = GenerateFile();

            var request = new AppealPostRequest
            {
                ApplicationId = applicationId,
                FileUpload = file,
                SelectedOption = AppealPostRequest.SubmitOption.Upload
            };

            await _controller.Appeal(request);

            _appealOrchestrator.Verify(x => x.UploadAppealFile(It.Is<Guid>(id => id == applicationId),
                It.Is<IFormFile>(f => f.FileName == file.FileName && f.ContentType == file.ContentType),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task Post_Appeal_RemoveFile_Removal_Is_Recorded()
        {
            var applicationId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            _appealOrchestrator.Setup(x =>
                x.RemoveAppealFile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));

            var request = new AppealPostRequest
            {
                ApplicationId = applicationId,
                SelectedOption = AppealPostRequest.SubmitOption.RemoveFile,
                FileId = fileId
            };
            
            await _controller.Appeal(request);

            _appealOrchestrator.Verify(x => x.RemoveAppealFile(applicationId, fileId, It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public async Task Post_Appeal_New_Appeal_Recorded()
        {
            var applicationId = Guid.NewGuid();
            var oversightReviewId = Guid.NewGuid();

            _appealOrchestrator.Setup(x =>
                x.CreateAppeal(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            
            var request = new AppealPostRequest
            {
                OversightReviewId = oversightReviewId,
                ApplicationId = applicationId,
                FileUpload = null,
                SelectedOption = AppealPostRequest.SubmitOption.SaveAndContinue,
                Message = _autoFixture.Create<string>()
            };

            await _controller.Appeal(request);

            _appealOrchestrator.Verify(x => x.CreateAppeal(It.Is<Guid>(id => id == applicationId),
                    It.Is<Guid>(reviewId => reviewId == oversightReviewId),
                    It.Is<string>(m => m == request.Message),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task Post_Appeal_User_Is_Redirected_To_Outcome_Page()
        {
            var applicationId = Guid.NewGuid();

            var request = new AppealPostRequest
            {
                ApplicationId = applicationId,
                Message = "This is an appeal",
                SelectedOption = AppealPostRequest.SubmitOption.SaveAndContinue
            };

            var result = await _controller.Appeal(request);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult) result;

            Assert.AreEqual("Outcome", redirectResult.ActionName);
            Assert.IsTrue(redirectResult.RouteValues.ContainsKey("ApplicationId"));
        }

        [Test]
        public async Task Get_Appeal_Preserves_Any_Message_Stored_In_TempData()
        {
            var request = new AppealRequest
            {
                ApplicationId = Guid.NewGuid()
            };

            var message = _autoFixture.Create<string>();

            _tempDataDictionary["Message"] = message;

            _appealOrchestrator.Setup(x =>
                    x.GetAppealViewModel(
                        It.Is<AppealRequest>(r => r.ApplicationId == request.ApplicationId),
                        It.Is<string>(m => m == message)))
                .ReturnsAsync(() => new AppealViewModel());

            var result = await _controller.Appeal(request);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [TestCase(AppealPostRequest.SubmitOption.Upload)]
        [TestCase(AppealPostRequest.SubmitOption.RemoveFile)]
        public async Task Post_Appeal_Preserves_Any_Message_In_TempData_On_Post_Of_Non_CTA(AppealPostRequest.SubmitOption submitOption)
        {
            var applicationId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            _appealOrchestrator.Setup(x =>
                x.RemoveAppealFile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()));

            var request = new AppealPostRequest
            {
                ApplicationId = applicationId,
                Message = _autoFixture.Create<string>(),
                FileUpload = submitOption == AppealPostRequest.SubmitOption.Upload ? GenerateFile() : null,
                FileId = submitOption == AppealPostRequest.SubmitOption.RemoveFile ? fileId : Guid.Empty,
                SelectedOption = submitOption
            };

            await _controller.Appeal(request);

            Assert.AreEqual(request.Message, _tempDataDictionary["Message"]);
        }

        [Test]
        public async Task Get_AppealFile_Returns_Uploaded_File()
        {
            var applicationId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var appealId = Guid.NewGuid();

            var fileUpload = new FileUpload
            {
                ContentType = "application/pdf",
                Data = _autoFixture.Create<byte[]>(),
                FileName = _autoFixture.Create<string>()
            };

            var request = new AppealUploadRequest
            {
                AppealId = appealId,
                AppealUploadId = fileId,
                ApplicationId = applicationId
            };

            _appealOrchestrator.Setup(x => x.GetAppealFile(applicationId, appealId, fileId)).ReturnsAsync(fileUpload);

            var result = await _controller.AppealUpload(request);

            Assert.IsInstanceOf<FileContentResult>(result);
            var fileContentResult = (FileContentResult) result;
            Assert.AreEqual(fileUpload.FileName, fileContentResult.FileDownloadName);
            Assert.AreEqual(fileUpload.ContentType, fileContentResult.ContentType);
            Assert.AreEqual(fileUpload.Data, fileContentResult.FileContents);
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
