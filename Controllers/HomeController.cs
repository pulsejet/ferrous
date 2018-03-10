using Ferrous.Misc;
using Microsoft.AspNetCore.Mvc;

namespace Ferrous.Controllers
{
    public class HomeController : ControllerBase
    {
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return File("Error.html", Utilities.HTML_MIME_TYPE);
        }
    }
}
