using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    [Authorize(Roles = Roles.RoatpApplicationOversightTeam)]
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
            var viewModel = await _oversightOrchestrator.GetApplicationsViewModel();
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
        public async Task<IActionResult> Outcome([CustomizeValidator(Interceptor = typeof(OutcomeValidatorInterceptor))]OutcomePostRequest request)
        {
            var userId = HttpContext.User.UserId();
            var userName = HttpContext.User.UserDisplayName();

            if (request.IsGatewayRemoved)
            {
                await _outcomeOrchestrator.RecordGatewayRemovedOutcome(request.ApplicationId, userId, userName);
                return RedirectToAction("Confirmed", new { request.ApplicationId });
            }

            if (request.IsGatewayFail)
            {
                await _outcomeOrchestrator.RecordGatewayFailOutcome(request.ApplicationId, userId, userName);
                return RedirectToAction("Confirmed", new {request.ApplicationId});
            }

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
            catch (InvalidStateException)
            {
                return RedirectToAction("Applications");
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

            var approveGateway = string.IsNullOrEmpty(request.ApproveGateway) ? default(bool?) : request.ApproveGateway == ApprovalStatus.Approve;
            var approveModeration = string.IsNullOrEmpty(request.ApproveModeration) ? default(bool?) : request.ApproveModeration == ApprovalStatus.Approve;

            await _outcomeOrchestrator.RecordOutcome(request.ApplicationId,  approveGateway, approveModeration, request.OversightStatus, userId, userName, request.InternalComments, request.ExternalComments);

            return RedirectToAction("Confirmed", new {request.ApplicationId});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/confirmed")]
        public async Task<IActionResult> Confirmed(ConfirmedRequest request)
        {
            var viewModel = await _oversightOrchestrator.GetConfirmedViewModel(request.ApplicationId);
            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{applicationId}/appeal")]
        public IActionResult Appeal(AppealRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
