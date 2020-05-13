using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    //[Authorize]
    public class OversightController: Controller
    {

        private readonly IOversightOrchestrator _orchestrator;
        private readonly ILogger<OversightController> _logger;

        public OversightController(IOversightOrchestrator orchestrator, ILogger<OversightController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task<IActionResult> Applications()
        {
            var viewModel = await _orchestrator.GetOversightOverviewViewModel();
            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> Outcome(Guid applicationId)
        {
            var vm = await _orchestrator.GetOversightDetailsViewModel(applicationId);
            return View(vm);
        }

        [HttpPost("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> EvaluateOutcome(Guid applicationId, string status)
        {
            var errorMessages = OversightValidator.ValidateOverallOutcome(status);
            var viewModel = await _orchestrator.GetOversightDetailsViewModel(applicationId);

            if (errorMessages.Any())
            {
                    viewModel.ErrorMessages = errorMessages;
                return View($"~/Views/Oversight/Outcome.cshtml", viewModel);
            }

            if (status == OversightReviewStatus.Successful)
            {

                var viewModelSuccessful = new OutcomeSuccessViewModel
                {
                    ApplicationId =  applicationId,
                    ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = DateTime.Today,
                    OrganisationName = viewModel.OrganisationName,
                    ProviderRoute = viewModel.ProviderRoute,
                    Ukprn = viewModel.Ukprn
                };
                return View("~/Views/Oversight/OutcomeSuccessful.cshtml",viewModelSuccessful);
            }

            var viewModelUnsuccessful = new OutcomeSuccessViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = DateTime.Today,
                OrganisationName = viewModel.OrganisationName,
                ProviderRoute = viewModel.ProviderRoute,
                Ukprn = viewModel.Ukprn
            };
            return View("~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModelUnsuccessful);
        }

        [HttpPost("Oversight/Outcome/Successful/{applicationId}")]
        public async Task<IActionResult> Successful(Guid applicationId, string status)
        {
            var errorMessages = OversightValidator.ValidateOutcomeSuccessful(status);
            var oversightViewModel = await _orchestrator.GetOversightDetailsViewModel(applicationId);

            var viewModel = new OutcomeSuccessViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = DateTime.Today,
                OrganisationName = oversightViewModel.OrganisationName,
                ProviderRoute = oversightViewModel.ProviderRoute,
                Ukprn = oversightViewModel.Ukprn
            };

            if (errorMessages.Any())
            {
                viewModel.ErrorMessages = errorMessages;
                return View($"~/Views/Oversight/OutcomeSuccessful.cshtml", viewModel);
            }
            

            if (status.ToLower()=="no")
            {
                oversightViewModel.ApplicationStatus = OversightReviewStatus.Successful;
                return View($"~/Views/Oversight/Outcome.cshtml", oversightViewModel);
            }


            // record in database it's a success
            var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Successful };

            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }

        [HttpPost("Oversight/Outcome/Unsuccessful/{applicationId}")]
        public async Task<IActionResult> Unsuccessful(Guid applicationId, string status)
        {
            var errorMessages = OversightValidator.ValidateOutcomeUnsuccessful(status);
            var oversightViewModel = await _orchestrator.GetOversightDetailsViewModel(applicationId);

            var viewModel = new OutcomeSuccessViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = DateTime.Today,
                OrganisationName = oversightViewModel.OrganisationName,
                ProviderRoute = oversightViewModel.ProviderRoute,
                Ukprn = oversightViewModel.Ukprn
            };

            if (errorMessages.Any())
            {
                viewModel.ErrorMessages = errorMessages;
                return View($"~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModel);
            }


            if (status.ToLower() == "no")
            {
                oversightViewModel.ApplicationStatus = OversightReviewStatus.Unsuccessful;
                return View($"~/Views/Oversight/Outcome.cshtml", oversightViewModel);
            }

            // record value in database

            var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Unsuccessful };

            return View("~/Views/Oversight/OutcomeDone.cshtml",viewModelDone);
        }
    }
}
