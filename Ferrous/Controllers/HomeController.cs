using Microsoft.AspNetCore.Mvc;

namespace Ferrous.Controllers
{
    public class HomeController : ControllerBase
    {
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (!Utilities.HasPrivilege(User.Identity.Name, 100))
                return File("account/login.html", Utilities.HTML_MIME_TYPE);
            return File("Index.html", Utilities.HTML_MIME_TYPE);
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return File("Error.html", Utilities.HTML_MIME_TYPE);
        }
    }
}
