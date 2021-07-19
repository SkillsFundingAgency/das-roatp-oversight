using System;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Extensions;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;
using SFA.DAS.RoatpOversight.Web.ModelBinders;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    [Authorize(Roles = Roles.RoatpApplicationOversightTeam)]
    public class OversightController : Controller
    {
        private readonly ISearchTermValidator _searchTermValidator;
        private readonly IApplicationOutcomeOrchestrator _outcomeOrchestrator;
        private readonly IOversightOrchestrator _oversightOrchestrator;
        private readonly IAppealOrchestrator _appealOrchestrator;

        public OversightController(ISearchTermValidator searchTermValidator,
                                   IApplicationOutcomeOrchestrator outcomeOrchestrator,
                                   IOversightOrchestrator oversightOrchestrator,
                                   IAppealOrchestrator appealOrchestrator)
        {
            _searchTermValidator = searchTermValidator;
            _outcomeOrchestrator = outcomeOrchestrator;
            _oversightOrchestrator = oversightOrchestrator;
            _appealOrchestrator = appealOrchestrator;
        }

        public async Task<IActionResult> Applications(string selectedTab, [StringTrim] string searchTerm, string sortColumn, string sortOrder)
        {
            if (searchTerm != null)
            {
                var validationResponse = _searchTermValidator.Validate(searchTerm);

                foreach (var error in validationResponse.Errors)
                {
                    ModelState.AddModelError(error.Field, error.ErrorMessage);
                }
            }

            var viewModel = await _oversightOrchestrator.GetApplicationsViewModel(selectedTab, ModelState.IsValid ? searchTerm : null, sortColumn, sortOrder);
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
        public async Task<IActionResult> Appeal(AppealRequest request)
        {
            var viewModel = await _appealOrchestrator.GetAppealViewModel(request, TempData.GetValue<string>("Message"));
            return View(viewModel);
        }

        [HttpPost("Oversight/Outcome/{applicationId}/appeal")]
        public async Task<IActionResult> Appeal(AppealPostRequest request)
        {
            var userId = HttpContext.User.UserId();
            var userName = HttpContext.User.UserDisplayName();

            if (request.SelectedOption == AppealPostRequest.SubmitOption.Upload)
            {
                await _appealOrchestrator.UploadAppealFile(request.ApplicationId, request.FileUpload, userId, userName);
                TempData.AddValue("Message", request.Message);
                return RedirectToAction("Appeal", new AppealRequest { ApplicationId = request.ApplicationId });
            }

            if (request.SelectedOption == AppealPostRequest.SubmitOption.RemoveFile)
            {
                await _appealOrchestrator.RemoveAppealFile(request.ApplicationId, request.FileId, userId, userName);
                TempData.AddValue("Message", request.Message);
                return RedirectToAction("Appeal", new AppealRequest { ApplicationId = request.ApplicationId });
            }

            await _appealOrchestrator.CreateAppeal(request.ApplicationId, request.OversightReviewId, request.Message, userId, userName);
            return RedirectToAction("Outcome", new OutcomeRequest {ApplicationId = request.ApplicationId});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/appeals/{appealId}/uploads/{appealUploadId}")]
        public async Task<IActionResult> AppealUpload(AppealUploadRequest request)
        {
            var file = await _appealOrchestrator.GetAppealFile(request.ApplicationId, request.AppealId,
                request.AppealUploadId);

            return File(file.Data,file.ContentType, file.FileName);
        }

    }
}
