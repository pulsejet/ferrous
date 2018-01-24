using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferrous.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!Utilities.HasPrivilege(User.Identity.Name))
                return Redirect(Utilities.LOGIN_URL);
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
