using System;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Validators
{
    [TestFixture]
    public class AppealPostRequestValidatorTests
    {
        private AppealPostRequestValidator _validator;
        private AppealPostRequest _request;

        [SetUp]
        public void SetUp()
        {
            _validator = new AppealPostRequestValidator();

            _request = new AppealPostRequest
            {
                ApplicationId = Guid.NewGuid(),
                Message = "This is a test message",
                SelectedOption = AppealPostRequest.SubmitOption.SaveAndContinue
            };
        }

        [Test]
        public void Validator_Returns_Valid()
        {
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
        public void Validator_Returns_Valid_When_AppealMessage_Is_Empty_And_User_Opted_To_Upload_Files()
        {
            _request.Message = string.Empty;
            _request.SelectedOption = AppealPostRequest.SubmitOption.Upload;

            var result = _validator.Validate(_request);
            
            Assert.IsTrue(result.IsValid);
        }
    }
}
