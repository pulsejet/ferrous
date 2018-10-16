using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ferrous.Controllers
{
    [Route("api/ext")]
    [Produces("application/json")]
    [ApiController]
    public class ExternalController : Controller
    {
        private readonly ferrousContext _context;

        public ExternalController(ferrousContext context)
        {
            _context = context;
        }

        [HttpPost("validate-form1")]
        [Misc.LinkRelation(Misc.LinkRelationList.overridden)]
        public async Task<IActionResult> ValidatePostForm1([FromBody] ExtContingentArrival extContingentArrival) {
            var contingent = await _context.Contingents.SingleOrDefaultAsync(m => m.ContingentLeaderNo == extContingentArrival.ContingentLeaderNo);
            if (contingent == null) {
                return BadRequest(new {message = "Not a contingent leader - " + extContingentArrival.ContingentLeaderNo});
            }

            var people = await _context.Person.ToListAsync();

            var contingentArrival = new ContingentArrival();
            contingentArrival.Male = extContingentArrival.Male;
            contingentArrival.Female = extContingentArrival.Female;
            contingentArrival.ContingentLeaderNo = extContingentArrival.ContingentLeaderNo;
            ContingentArrivalsController contingentArrivalsController = new ContingentArrivalsController(_context);

            /* Make CAPerson entries */
            foreach (string mino in extContingentArrival.Minos) {
                var caPerson = new CAPerson();
                caPerson.Mino = mino;
                caPerson.CANav = contingentArrival;
                caPerson.person = people.SingleOrDefault(m => m.Mino == mino);
                contingentArrivalsController.FillCAPerson(caPerson, people.ToArray(), contingent.ContingentLeaderNo, false);
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
            var contingentArrival = new ContingentArrival();
            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            contingentArrival.Male = extContingentArrival.Male;
            contingentArrival.Female = extContingentArrival.Female;
            contingentArrival.ContingentLeaderNo = extContingentArrival.ContingentLeaderNo;
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

            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        public class ExtContingentArrival {
            public string ContingentLeaderNo {get; set; }
            public string FillerMINo {get; set; }
            public int Male {get; set; }
            public int Female {get; set; }
            public string[] Minos {get; set; }
            public string[] OnSpotMinos {get; set; }
        }

    }
}
