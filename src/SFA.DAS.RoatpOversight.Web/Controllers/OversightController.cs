using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;
using SFA.DAS.RoatpOversight.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using SFA.DAS.RoatpOversight.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    [Authorize]
    public class OversightController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IApplicationOutcomeOrchestrator _outcomeOrchestrator;
        
        private IWebConfiguration _configuration;
        private readonly IOversightOrchestrator _oversightOrchestrator;
        private readonly ILogger<OversightController> _logger;
        public OversightController(IHttpContextAccessor contextAccessor, IApplicationOutcomeOrchestrator outcomeOrchestrator, IWebConfiguration configuration,
                                   IOversightOrchestrator oversightOrchestrator, ILogger<OversightController> logger)
        {
            _contextAccessor = contextAccessor;
            _outcomeOrchestrator = outcomeOrchestrator;
            _configuration = configuration;
            _oversightOrchestrator = oversightOrchestrator;
            _logger = logger;
        }

        public async Task<IActionResult> Applications()
        {
            var viewModel = await _oversightOrchestrator.GetOversightOverviewViewModel();
            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> Outcome(Guid applicationId)
        {
            var vm = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId);
            var oversightStatus = vm.OversightStatus;

            if (CheckForBackButtonAfterSubmission(oversightStatus, out var outcome)) return outcome;
 
             return View(vm);
        }


        [HttpPost("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> EvaluateOutcome(Guid applicationId, string status)
        {
            var viewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId);

            if (CheckForBackButtonAfterSubmission(viewModel.OversightStatus, out var outcome)) return outcome;

            var errorMessages = OversightValidator.ValidateOverallOutcome(status);
       
            if (errorMessages.Any())
            {
                    viewModel.ErrorMessages = errorMessages;
                return View($"~/Views/Oversight/Outcome.cshtml", viewModel);
            }

            var viewModelSuccess = new OutcomeSuccessStatusViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = viewModel.ApplicationSubmittedDate,
                OrganisationName = viewModel.OrganisationName,
                ProviderRoute = viewModel.ProviderRoute,
                Ukprn = viewModel.Ukprn
            };

            if (status == OversightReviewStatus.Successful)
            {
                return View("~/Views/Oversight/OutcomeSuccessful.cshtml",viewModelSuccess);
            }

            return View("~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModelSuccess);
        }

        [HttpPost("Oversight/Outcome/Successful/{applicationId}")]
        public async Task<IActionResult> Successful(Guid applicationId, string status)
        {
            var oversightViewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId);
            if (CheckForBackButtonAfterSubmission(oversightViewModel.OversightStatus, out var outcome)) return outcome;
            var errorMessages = OversightValidator.ValidateOutcomeSuccessful(status);

            var viewModel = new OutcomeSuccessStatusViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = oversightViewModel.ApplicationSubmittedDate,
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

            await _outcomeOrchestrator.RecordOutcome(applicationId, OversightReviewStatus.Successful, _contextAccessor.HttpContext.User.UserDisplayName());

            // record in database it's a success
            var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Successful };

            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }

        [HttpPost("Oversight/Outcome/Unsuccessful/{applicationId}")]
        public async Task<IActionResult> Unsuccessful(Guid applicationId, string status)
        {
            var oversightViewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId);
            if (CheckForBackButtonAfterSubmission(oversightViewModel.OversightStatus, out var outcome)) return outcome;
            var errorMessages = OversightValidator.ValidateOutcomeUnsuccessful(status);

            var viewModel = new OutcomeSuccessStatusViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
                ApplicationSubmittedDate = oversightViewModel.ApplicationSubmittedDate,
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

            await _outcomeOrchestrator.RecordOutcome(applicationId, OversightReviewStatus.Unsuccessful, _contextAccessor.HttpContext.User.UserDisplayName());

            var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Unsuccessful };

            return View("~/Views/Oversight/OutcomeDone.cshtml",viewModelDone);
        }


        private bool CheckForBackButtonAfterSubmission(string oversightStatus, out IActionResult outcome)
        {
            outcome = null;
            if (oversightStatus == OversightReviewStatus.Successful || oversightStatus == OversightReviewStatus.Unsuccessful)
            {
                {
                    outcome = new RedirectToActionResult("Applications", "Oversight", null);
                    return true;
                }
            }

            return false;
        }
    }
}
