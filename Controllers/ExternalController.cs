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

        [HttpPost("form1")]
        public async Task<IActionResult> PostForm1([FromBody] ExtContingentArrival extContingentArrival) {

            var contingent = await _context.Contingents.SingleOrDefaultAsync(m => m.ContingentLeaderNo == extContingentArrival.ContingentLeaderNo);
            if (contingent == null) {
                return BadRequest(new {message = "Not a contingent leader - " + extContingentArrival.ContingentLeaderNo});
            }

            var contingentArrival = new ContingentArrival();
            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            contingentArrival.Male = extContingentArrival.Male;
            contingentArrival.Female = extContingentArrival.Female;
            contingentArrival.ContingentLeaderNo = extContingentArrival.ContingentLeaderNo;
            _context.ContingentArrival.Add(contingentArrival);
            await _context.SaveChangesAsync();

            foreach (string mino in extContingentArrival.Minos) {
                var caPerson = new CAPerson();
                caPerson.Mino = mino;
                caPerson.ContingentArrivalNavigation = contingentArrival;
                _context.CAPerson.Add(caPerson);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        public class ExtContingentArrival {
            public string ContingentLeaderNo;
            public int Male;
            public int Female;
            public string[] Minos;
            public string[] OnSpotMinos;
        }

    }
}
