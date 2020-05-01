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
        public async Task ViewApplication_returns_view_with_expected_viewmodel()
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

            // act
            var result = await _controller.Applications() as ViewResult;
            var actualViewModel = result?.Model as OverallOutcomeViewModel;

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(actualViewModel, Is.Not.Null);
            Assert.That(actualViewModel, Is.SameAs(viewModel));
            Assert.AreEqual(_applicationDetailsApplicationId, actualViewModel.ApplicationDetails.FirstOrDefault().ApplicationId);
            Assert.AreEqual(_ukprnOfCompletedOversightApplication, actualViewModel.OverallOutcomeDetails.FirstOrDefault().Ukprn);
        }
    }
}
