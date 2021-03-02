using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Validators
{
    [TestFixture]
    public class AppealPostRequestValidatorTests
    {
        private AppealPostRequestValidator _validator;
        private AppealPostRequest _request;
        private Mock<IPdfValidatorService> _pdfValidatorService;

        [SetUp]
        public void SetUp()
        {
            _pdfValidatorService = new Mock<IPdfValidatorService>();
            _pdfValidatorService.Setup(x => x.IsPdf(It.IsAny<IFormFile>())).ReturnsAsync(true);

            _validator = new AppealPostRequestValidator(_pdfValidatorService.Object);

            _request = new AppealPostRequest
            {
                ApplicationId = Guid.NewGuid(),
                Message = "This is a test message",
                SelectedOption = AppealPostRequest.SubmitOption.SaveAndContinue
            };
        }

        [Test]
        public void Validator_Returns_Valid_On_Save_And_Continue()
        {
            var result = _validator.Validate(_request);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validator_Returns_Valid_On_File_Upload()
        {
            _request.SelectedOption = AppealPostRequest.SubmitOption.Upload;
            _request.FileUpload = GenerateMockFile(1).Object;
            _request.Message = null;

            var result = _validator.Validate(_request);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validator_Returns_Error_When_AppealMessage_Is_Empty()
        {
            _request.Message = string.Empty;

            var result = _validator.Validate(_request);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == nameof(_request.Message)));
        }

        [Test]
        public void Validator_Returns_Invalid_When_Uploading_Without_Selecting_A_File()
        {
            _request.SelectedOption = AppealPostRequest.SubmitOption.Upload;
            _request.FileUpload = null;

            var result = _validator.Validate(_request);

            Assert.IsFalse(result.IsValid);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(5242880, true)]
        [TestCase(5242881, false)]
        public void Validator_Returns_Invalid_When_Uploading_A_File_That_Is_An_Invalid_Size(int size, bool expectIsValid)
        {
            _request.SelectedOption = AppealPostRequest.SubmitOption.Upload;
            _request.FileUpload = GenerateMockFile(size).Object;

            var result = _validator.Validate(_request);

            Assert.AreEqual(expectIsValid, result.IsValid);
        }

        [Test]
        public void Validator_Returns_Invalid_When_Uploading_A_File_That_Is_Not_A_PDF()
        {
            _pdfValidatorService.Setup(x => x.IsPdf(It.IsAny<IFormFile>())).ReturnsAsync(() => false);

            _request.SelectedOption = AppealPostRequest.SubmitOption.Upload;
            _request.FileUpload = GenerateMockFile(1).Object;

            var result = _validator.Validate(_request);

            Assert.IsFalse(result.IsValid);
        }

        private static Mock<IFormFile> GenerateMockFile(int size)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.Length).Returns(size);
            return fileMock;
        }
    }
}
