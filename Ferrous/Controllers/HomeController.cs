using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ferrous.Controllers
{
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            if (!Utilities.HasPrivilege(User.Identity.Name, 100))
                return File("account/login.html", Utilities.HTML_MIME_TYPE);
            return File("Index.html", Utilities.HTML_MIME_TYPE);
        }

        public IActionResult Error()
        {
            return File("Error.html", Utilities.HTML_MIME_TYPE);
        }
    }
}
