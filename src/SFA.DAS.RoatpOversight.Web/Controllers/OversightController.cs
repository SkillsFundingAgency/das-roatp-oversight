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
            var errorMessages = OverallOutcomeValidator.ValidateOverallOutcome(status);
            var viewModel = await _orchestrator.GetOversightDetailsViewModel(applicationId);

            if (errorMessages.Any())
            {
                    viewModel.ErrorMessages = errorMessages;
                return View($"~/Views/Oversight/Outcome.cshtml", viewModel);
            }

            if (status.ToLower() == "successful")
            {

                var viewModelSuccessful = new OutcomeSuccessfulViewModel
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

            var viewModelUnsuccessful = new OutcomeUnsuccessfulViewModel
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
        public IActionResult Successful(Guid applicationId, string status)
        {
            var viewModel = new OutcomeSuccessfulViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = "DUMMY REFERENCE NUMBER",
                ApplicationSubmittedDate = DateTime.Today,
                OrganisationName = "THIS IS A DUMMY PAGE",
                ProviderRoute = "DUMMY ROUTE",
                Ukprn = "DUMMY UKPRN"
            };

            viewModel.ErrorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(status))
            {
                viewModel.ErrorMessages.Add(new ValidationErrorDetail
                { ErrorMessage = "error message", ValidationStatusCode = new ValidationStatusCode() });

                return View($"~/Views/Oversight/OutcomeSuccessful.cshtml", viewModel);
            }

            if (status.ToLower()=="no")
            {
                var viewModelOutcome = new OutcomeViewModel
                {
                    ApplicationId = applicationId,
                    ApplicationReferenceNumber = "DUMMY REFERENCE NUMBER",
                    ApplicationSubmittedDate = DateTime.Today,
                    OrganisationName = "THIS IS A DUMMY PAGE",
                    ProviderRoute = "DUMMY ROUTE",
                    Ukprn = "DUMMY UKPRN",
                    ApplicationStatus = "Successful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }


            // record in database it's a success
            var viewModelDone = new OutcomeDoneViewModel { Ukprn ="DUMMY UKPRN", Status = "Successful" };

            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }

        [HttpPost("Oversight/Outcome/Unsuccessful/{applicationId}")]
        public IActionResult Unsuccessful(Guid applicationId, string status)
        {
          
            var viewModel = new OutcomeUnsuccessfulViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = "DUMMY REFERENCE NUMBER",
                ApplicationSubmittedDate = DateTime.Today,
                OrganisationName = "THIS IS A DUMMY PAGE",
                ProviderRoute = "DUMMY ROUTE",
                Ukprn = "DUMMY UKPRN"
            };

            viewModel.ErrorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(status))
            {
                viewModel.ErrorMessages.Add(new ValidationErrorDetail
                    { ErrorMessage = "error message", ValidationStatusCode = new ValidationStatusCode() });

                return View($"~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModel);
            }

            if (status.ToLower() == "no")
            {
                var viewModelOutcome = new OutcomeViewModel
                {
                    ApplicationReferenceNumber = "DUMMY REFERENCE NUMBER",
                    ApplicationSubmittedDate = DateTime.Today,
                    OrganisationName = "THIS IS A DUMMY PAGE",
                    ProviderRoute = "DUMMY ROUTE",
                    Ukprn = "DUMMY UKPRN",
                    ApplicationStatus = "Unsuccessful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }

            // record value in database

            var viewModelDone = new OutcomeDoneViewModel {Ukprn = "DUMMY UKPRN", Status = "Unsuccessful"};

                return View("~/Views/Oversight/OutcomeDone.cshtml",viewModelDone);
        }
    }
}
