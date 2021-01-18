using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly IWebConfiguration _configuration;

        public HomeController(IWebConfiguration configuration)
        {
            _configuration = configuration;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        public IActionResult Index()
        {
            return RedirectToAction("Applications", "Oversight");
        }

        [Route("/Dashboard")]
        public IActionResult Dashboard()
        {
            return Redirect(_configuration.EsfaAdminServicesBaseUrl + "/Dashboard");
        }
    }
}
