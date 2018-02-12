using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Controllers.Authorization;
using static Ferrous.Controllers.Utilities;

namespace Ferrous.Controllers
{
    [Route("api/login")]
    public class LoginController: ControllerBase
    {
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return Redirect("/account/login.html");
        }

        [HttpGet("login")]
        public async Task<IActionResult> login()
        {
            string username = Request.Query["username"];
            string password = Request.Query["password"];

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLower() == username.ToLower());

            if (id!=null && id.password == Utilities.SHA.GenerateSHA256String(id.salt + password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, id.username)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(4)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Ok();
            }
            
            return Unauthorized();
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/account/login.html");
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("getuser")]
        public IActionResult GetUser()
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            return Content(User.Identity.Name);
        }
    }
}
