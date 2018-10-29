using System;
using System.Collections.Generic;
using System.Linq;
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

            /* Make CAPerson entries */
            foreach (string mino in extContingentArrival.Minos) {
                var caPerson = new CAPerson();
                caPerson.Mino = mino;
                caPerson.CANav = contingentArrival;
                _context.CAPerson.Add(caPerson);
            }

            await _context.SaveChangesAsync();

            DataUtilities.UpdateWebSock(null, _hubContext);
            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        private ContingentArrival fillFromExt(ExtContingentArrival extContingentArrival)  {
            var contingentArrival = new ContingentArrival();
            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            contingentArrival.Male = extContingentArrival.Male;
            contingentArrival.Female = extContingentArrival.Female;
            contingentArrival.MaleOnSpotDemand = extContingentArrival.MaleOnSpotDemand;
            contingentArrival.FemaleOnSpotDemand = extContingentArrival.FemaleOnSpotDemand;
            contingentArrival.ContingentLeaderNo = extContingentArrival.ContingentLeaderNo;
            return contingentArrival;
        }

        public class ExtContingentArrival {
            public string ContingentLeaderNo {get; set; }
            public string FillerMINo {get; set; }
            public int Male {get; set; }
            public int Female {get; set; }
            public int MaleOnSpotDemand {get; set; }
            public int FemaleOnSpotDemand {get; set; }
            public string[] Minos {get; set; }
            public string[] OnSpotMinos {get; set; }
        }

    }
}
