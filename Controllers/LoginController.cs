using Ferrous.Misc;
using Microsoft.AspNetCore.Authentication;
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
        public static bool PasswordMatches(FerrousIdentity id, String password) {
            return (id != null && id.password == Misc.Utilities.SHA.GenerateSHA256String(id.salt + password));
        }

        [HttpGet("login")]
        public async Task<IActionResult> login([FromQuery] string username, [FromQuery] string password)
        {
            Response.Headers.Add("Content-Type", "application/octet-stream");

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLowerInvariant() == username.ToLowerInvariant());

            if (PasswordMatches(id, password))
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
            if (!User.Identity.IsAuthenticated) { return Unauthorized(); }

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username.ToLower() == User.Identity.Name.ToLower());
            id.password = null;
            id.salt = null;

            return new JsonResult(id);
        }

        private const string HIDDEN_FIELD = "HIDDEN";

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("all-users")]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.MANAGE_USERS)]
        public IActionResult GetAllUsers()
        {
            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            foreach (var i in identities) {
                i.salt = i.password = HIDDEN_FIELD;
            }
            return new JsonResult(identities);
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost("all-users")]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.MANAGE_USERS)]
        public IActionResult PostAllUsers([FromBody] List<FerrousIdentity> users, [FromQuery] String password)
        {
            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(
                m => m.username.ToLowerInvariant() == HttpContext.User.Identity.Name.ToLowerInvariant());

            /* Check password */
            if (!PasswordMatches(id, password)) {
                return Unauthorized();
            }

            /* Form new identities json */
            foreach (var i in users) {
                if (i.username == null || i.username == String.Empty) {
                    return BadRequest(new { message = "Blank username not allowed"});
                }

                if (i.password != null && i.password != HIDDEN_FIELD && i.password != String.Empty) {
                    i.salt = Utilities.RandomString(2);
                    i.password = Misc.Utilities.SHA.GenerateSHA256String(i.salt + i.password);
                } else {
                    var cid = identities.FirstOrDefault(m => m.username == i.username);
                    if (cid != null) {
                        i.salt = cid.salt;
                        i.password = cid.password;
                    } else {
                        return BadRequest(new { message = $"Blank password not allowed for new user {i.username}"});
                    }
                }
            }
            Utilities.WriteJson(IDENTITIES_JSON_FILE, users);
            Authorization.reloadIdentities();
            return NoContent();
        }

        [HttpGet("all-privileges")]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.MANAGE_USERS)]
        public IActionResult GetAllPrivileges() {
            int[] values = (int[]) Enum.GetValues(typeof(PrivilegeList));
            var result = new Dictionary<int, string>();
            foreach (int val in values) {
                PrivilegeList privilege = (PrivilegeList) val;
                result[val] = privilege.ToString();
            }
            return Ok(result);
        }
    }
}
