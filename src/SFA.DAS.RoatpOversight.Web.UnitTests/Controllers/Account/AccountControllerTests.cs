﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Web.Controllers;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Account
{
    [TestFixture]
    public class AccountControllerTests
    {
        private AccountController _controller;
        private Mock<IWebConfiguration> _configurationMock;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IWebConfiguration>();
            _configurationMock.Setup(x => x.UseDfeSignIn).Returns(true);
            _configurationMock.Setup(x => x.DfESignInServiceHelpUrl).Returns("test");
            _controller = new AccountController(Mock.Of<ILogger<AccountController>>(), _configurationMock.Object)
            {
                ControllerContext = MockedControllerContext.Setup(),
                Url = Mock.Of<IUrlHelper>()
            };
        }

        [Test]
        public void SignIn_returns_expected_ChallengeResult()
        {
            _configurationMock.Setup(x => x.UseDfeSignIn).Returns(false);
            
            var result = _controller.SignIn() as ChallengeResult;

            Assert.That(result, Is.Not.Null);
            CollectionAssert.IsNotEmpty(result.AuthenticationSchemes);
            CollectionAssert.Contains(result.AuthenticationSchemes, WsFederationDefaults.AuthenticationScheme);
        }
        
        [Test]
        public void SignIn_returns_expected_ChallengeResult_DfeSignIn()
        {
            _configurationMock.Setup(x => x.UseDfeSignIn).Returns(true);
            
            var result = _controller.SignIn() as ChallengeResult;

            Assert.That(result, Is.Not.Null);
            CollectionAssert.IsNotEmpty(result.AuthenticationSchemes);
            CollectionAssert.Contains(result.AuthenticationSchemes, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Test]
        public void PostSignIn_redirects_to_Home()
        {
            var result = _controller.PostSignIn() as RedirectToActionResult;

            Assert.AreEqual("Home", result.ControllerName);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void SignOut_returns_expected_SignOutResult_For_Pirean()
        {
            _configurationMock.Setup(x => x.UseDfeSignIn).Returns(false);
                
            var result = _controller.SignOut() as SignOutResult;

            Assert.That(result, Is.Not.Null);
            CollectionAssert.IsNotEmpty(result.AuthenticationSchemes);
            CollectionAssert.Contains(result.AuthenticationSchemes, WsFederationDefaults.AuthenticationScheme);
            CollectionAssert.Contains(result.AuthenticationSchemes, CookieAuthenticationDefaults.AuthenticationScheme);
        }
        [Test]
        public void SignOut_returns_expected_SignOutResult_For_DfeSignIn()
        {
            _configurationMock.Setup(x => x.UseDfeSignIn).Returns(true);
                
            var result = _controller.SignOut() as SignOutResult;

            Assert.That(result, Is.Not.Null);
            CollectionAssert.IsNotEmpty(result.AuthenticationSchemes);
            CollectionAssert.Contains(result.AuthenticationSchemes, OpenIdConnectDefaults.AuthenticationScheme);
            CollectionAssert.Contains(result.AuthenticationSchemes, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [Test]
        public void SignedOut_shows_correct_view()
        {
            var result = _controller.SignedOut() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("SignedOut", result.ViewName);
        }

        [Test]
        public void AccessDenied_shows_correct_view()
        {
            var result = _controller.AccessDenied() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("AccessDenied", result.ViewName);
            var actualModel = result.Model as Error403ViewModel;
            Assert.NotNull(actualModel);
            Assert.True(actualModel.UseDfESignIn);
            Assert.AreEqual("test", actualModel.HelpPageLink);
        }
    }
}
