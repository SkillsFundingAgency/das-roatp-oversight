﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IWebConfiguration _webConfiguration;

        public AccountController(ILogger<AccountController> logger, IWebConfiguration webConfiguration)
        {
            _logger = logger;
            _webConfiguration = webConfiguration;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            var challengeScheme = _webConfiguration.UseDfeSignIn
                ? OpenIdConnectDefaults.AuthenticationScheme
                : WsFederationDefaults.AuthenticationScheme;
            _logger.LogInformation("Start of Sign In");
            var redirectUrl = Url.Action("PostSignIn", "Account");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                challengeScheme);
        }

        [HttpGet]
        public IActionResult PostSignIn()
        {
            //if (!HttpContext.User.HasValidRole())
            //{
            //    _logger.LogInformation($"PostSignIn - User '{HttpContext.User.Identity.Name}' does not have a valid role");
            //    foreach (var cookie in Request.Cookies.Keys)
            //    {
            //        Response.Cookies.Delete(cookie);
            //    }

            //    return RedirectToAction("AccessDenied");
            //}

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action("SignedOut", "Account", values: null, protocol: Request.Scheme);

            var authScheme = _webConfiguration.UseDfeSignIn
                ? OpenIdConnectDefaults.AuthenticationScheme
                : WsFederationDefaults.AuthenticationScheme;

            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                authScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            return View("SignedOut");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (HttpContext.User != null)
            {
                var userName = HttpContext.User.Identity.Name ?? HttpContext.User.FindFirstValue(ClaimTypes.Upn);
                var roles = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == Domain.Roles.RoleClaimType).Select(c => c.Value);

                _logger.LogError($"AccessDenied - User '{userName}' does not have a valid role. They have the following roles: '{string.Join(",", roles)}'");
            }
            var model = new Error403ViewModel
            {
                UseDfESignIn = _webConfiguration.UseDfeSignIn,
                HelpPageLink = _webConfiguration.DfESignInServiceHelpUrl
            };

            return View("AccessDenied",model);
        }
    }
}
