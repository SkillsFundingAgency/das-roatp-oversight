using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Home
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private Mock<IWebConfiguration> _configuration;
        private string _dashboardUrl;

       
        [SetUp]
        public void SetUp()
        {
            _dashboardUrl = "https://dashboard";
            _configuration = new Mock<IWebConfiguration>();
            _configuration.Setup(c => c.EsfaAdminServicesBaseUrl).Returns(_dashboardUrl);
            _controller = new HomeController(_configuration.Object)
            {
                ControllerContext = MockedControllerContext.Setup()
            };
        }

        [Test]
        public void Error_returns_view_with_expected_viewmodel()
        {
            var expectedViewModel = new ErrorViewModel { RequestId = _controller.HttpContext.TraceIdentifier };

            var result = _controller.Error() as ViewResult;
            var actualViewModel = result?.Model as ErrorViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);           
            Assert.That(actualViewModel.RequestId, Is.EqualTo(expectedViewModel.RequestId));
            Assert.That(actualViewModel.ShowRequestId, Is.EqualTo(!string.IsNullOrEmpty(expectedViewModel.RequestId)));
        }

        [Test]
        public void Dashboard_redirects_to_external_dasbhoard_url()
        {
            var result = _controller.Dashboard() as RedirectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Url, $"{_dashboardUrl}/Dashboard");
        }
    }
}
