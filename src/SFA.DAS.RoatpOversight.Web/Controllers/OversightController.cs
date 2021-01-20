using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    [Authorize]
    public class OversightController : Controller
    {
        private readonly IApplicationOutcomeOrchestrator _outcomeOrchestrator;
        private readonly IOversightOrchestrator _oversightOrchestrator;

        public OversightController(IApplicationOutcomeOrchestrator outcomeOrchestrator,
                                   IOversightOrchestrator oversightOrchestrator)
        {
            _outcomeOrchestrator = outcomeOrchestrator;
            _oversightOrchestrator = oversightOrchestrator;
        }

        public async Task<IActionResult> Applications()
        {
            var viewModel = await _oversightOrchestrator.GetOversightOverviewViewModel();
            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> Outcome(OutcomeRequest request)
        {
            try
            {
                var vm = await _oversightOrchestrator.GetOversightDetailsViewModel(request.ApplicationId, request.OutcomeKey);
                return View(vm);
            }
            catch(InvalidStateException)
            {
                return RedirectToAction("Applications");
            }
        }

        [HttpPost("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> Outcome(OutcomePostRequest request)
        {
            var cacheKey = await _oversightOrchestrator.SaveOutcomePostRequestToCache(request);
            return RedirectToAction("ConfirmOutcome", new {applicationId = request.ApplicationId, OutcomeKey = cacheKey});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
        public async Task<IActionResult> ConfirmOutcome(ConfirmOutcomeRequest request)
        {
            try
            {
                var viewModel = await _oversightOrchestrator.GetConfirmOutcomeViewModel(request.ApplicationId, request.OutcomeKey);
                return View(viewModel);
            }
            catch (ConfirmOutcomeCacheKeyNotFoundException)
            {
                return RedirectToAction("Outcome", new { request.ApplicationId });
            }
        }

        [HttpPost("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
        public async Task<IActionResult> ConfirmOutcome(ConfirmOutcomePostRequest request)
        {
            if (request.Confirm == OversightConfirmationStatus.No)
            {
                return RedirectToAction("Outcome", new { request.ApplicationId, request.OutcomeKey });
            }

            var userId = HttpContext.User.UserId();
            var userName = HttpContext.User.UserDisplayName();

            await _outcomeOrchestrator.RecordOutcome(request.ApplicationId, request.OversightStatus, userId, userName);

            return RedirectToAction("Confirmed", new {request.ApplicationId});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/confirmed")]
        public async Task<IActionResult> Confirmed(ConfirmedRequest request)
        {
            var viewModel = await _oversightOrchestrator.GetConfirmedViewModel(request.ApplicationId);
            return View(viewModel);
        }
    }
}
