using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Ferrous.Misc;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Ferrous.Controllers
{
    [Route("api/ext")]
    [Produces("application/json")]
    [ApiController]
    public class ExternalController : Controller
    {
        private readonly ferrousContext _context;
        private readonly IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        public ExternalController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("validate-form1")]
        [Misc.LinkRelation(Misc.LinkRelationList.overridden)]
        public async Task<IActionResult> ValidatePostForm1([FromBody] ExtContingentArrival extContingentArrival) {
            var contingent = await _context.Contingents.SingleOrDefaultAsync(m => m.ContingentLeaderNo == extContingentArrival.ContingentLeaderNo);
            if (contingent == null) {
                return BadRequest(new {message = "Not a contingent leader - " + extContingentArrival.ContingentLeaderNo});
            }

            var people = await _context.Person.ToListAsync();

            var contingentArrival = fillFromExt(extContingentArrival);

            /* Make CAPerson entries */
            foreach (string mino in extContingentArrival.Minos) {
                var caPerson = new CAPerson();
                caPerson.Mino = mino;
                caPerson.CANav = contingentArrival;
                caPerson.person = people.SingleOrDefault(m => m.Mino == mino);
                DataUtilities.FillCAPerson(User, Url, caPerson, people.ToArray(), contingent.ContingentLeaderNo, false);
                contingentArrival.CAPeople.Add(caPerson);
            }

            return Ok(contingentArrival);
        }

        [HttpPost("form1")]
        [Misc.LinkRelation(Misc.LinkRelationList.overridden)]
        public async Task<IActionResult> PostForm1([FromBody] ExtContingentArrival extContingentArrival) {

            /* Check if contingent exists */
            var contingent = await _context.Contingents.SingleOrDefaultAsync(m => m.ContingentLeaderNo == extContingentArrival.ContingentLeaderNo);
            if (contingent == null) {
                return BadRequest(new {message = "Not a contingent leader - " + extContingentArrival.ContingentLeaderNo});
            }

            /* Add contingent arrival */
            var contingentArrival = fillFromExt(extContingentArrival);
            _context.ContingentArrival.Add(contingentArrival);
            await _context.SaveChangesAsync();

            var people = await _context.Person.ToListAsync();

            /* Make CAPerson entries */
            foreach (string mino in extContingentArrival.Minos) {
                var caPerson = new CAPerson();
                caPerson.Mino = mino;
                caPerson.CANav = contingentArrival;
                caPerson.person = people.SingleOrDefault(m => m.Mino == caPerson.Mino);
                DataUtilities.FillCAPerson(User, Url, caPerson, people.ToArray(), contingent.ContingentLeaderNo, false);
                _context.CAPerson.Add(caPerson);
            }

            await _context.SaveChangesAsync();

            /* Send email */
            try {
                StringBuilder body = new StringBuilder();
                body.Append($"Hello {extContingentArrival.FillerMiNo}!\n\n");
                body.Append($"Your request for accommodation has been registered");
                body.Append($"and the token number allotted to you is {contingentArrival.ContingentArrivalNo}.\n\n");
                body.Append($"Proceed to Hospitality Desk 1 for further action.");
                body.Append($"Keep this token number and ID cards of all members with you availing accommodation ready.\n\n");

                body.Append($"Enlisted below are the details filled by you:\n");
                body.Append($"Male: {contingentArrival.Male}\n");
                body.Append($"Female: {contingentArrival.Female}\n");
                body.Append($"On-Spot Requests (Male): {contingentArrival.MaleOnSpotDemand}\n");
                body.Append($"On-Spot Requests (Female): {contingentArrival.FemaleOnSpotDemand}\n\n");

                foreach (var caPerson in contingentArrival.CAPeople) {
                    body.Append($"{caPerson.Mino}");
                    string flags = caPerson.flags == "" ? "OK" : caPerson.flags;
                    if (caPerson.person != null) {
                        body.Append($"- {caPerson.person.Name} - {caPerson.person.Sex}");
                    }
                    body.Append($" - {flags}\n");
                }

                SmtpClient client = new SmtpClient("localhost");
                client.UseDefaultCredentials = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("accommodation@moodi.org", "Mood Indigo");
                mailMessage.To.Add(extContingentArrival.FillerEmail);
                mailMessage.Bcc.Add("accommodation@moodi.org");
                mailMessage.Body = body.ToString();
                mailMessage.Subject = $"Accommodation Token #{contingentArrival.ContingentArrivalNo}";
                client.SendAsync(mailMessage, null);
            } catch {
                // No email sent :(
            }

            DataUtilities.UpdateWebSock(null, _hubContext);
            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        private ContingentArrival fillFromExt(ExtContingentArrival extContingentArrival)  {
            var contingentArrival = new ContingentArrival();
            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            contingentArrival.FillerMiNo = extContingentArrival.FillerMiNo;
            contingentArrival.Male = extContingentArrival.Male;
            contingentArrival.Female = extContingentArrival.Female;
            contingentArrival.MaleOnSpotDemand = extContingentArrival.MaleOnSpotDemand;
            contingentArrival.FemaleOnSpotDemand = extContingentArrival.FemaleOnSpotDemand;
            contingentArrival.ContingentLeaderNo = extContingentArrival.ContingentLeaderNo;
            return contingentArrival;
        }

        public class ExtContingentArrival {
            public string ContingentLeaderNo {get; set; }
            public string FillerMiNo {get; set; }
            public string FillerEmail {get; set; }
            public int Male {get; set; }
            public int Female {get; set; }
            public int MaleOnSpotDemand {get; set; }
            public int FemaleOnSpotDemand {get; set; }
            public string[] Minos {get; set; }
            public string[] OnSpotMinos {get; set; }
        }

    }
}
