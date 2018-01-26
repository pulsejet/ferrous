using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ferrous.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!Utilities.HasPrivilege(User.Identity.Name, 100))
                return View("Login");
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
