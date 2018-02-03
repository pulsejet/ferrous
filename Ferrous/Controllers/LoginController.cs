using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Utilities;

namespace Ferrous.Controllers
{
    [Route("Login")]
    public class LoginController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("login")]
        public async Task<IActionResult> login()
        {
            string username = Request.Query["username"];
            string password = Request.Query["password"];

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLower() == username.ToLower());

            if (id!=null && id.password == password)
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, id.username));
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(4)
                    });
                return Ok();
            }
            
            return Unauthorized();
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return File("account/login.html", Utilities.HTML_MIME_TYPE);
        }
    }
}
