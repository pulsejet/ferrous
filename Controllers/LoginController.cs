﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Misc.Authorization;
using static Ferrous.Misc.Utilities;

namespace Ferrous.Controllers
{
    [Route("api/login")]
    public class LoginController: ControllerBase
    {
        [HttpGet("login")]
        public async Task<IActionResult> login([FromQuery] string username, [FromQuery] string password)
        {
            Response.Headers.Add("Content-Type", "application/octet-stream");

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLowerInvariant() == username.ToLowerInvariant());

            if (id!=null && id.password == Misc.Utilities.SHA.GenerateSHA256String(id.salt + password))
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

                return NoContent();
            }
            
            return Unauthorized();
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("getuser")]
        public IActionResult GetUser()
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLower() == User.Identity.Name.ToLower());
            id.password = id.salt = null;

            return new JsonResult(id);
        }
    }
}
