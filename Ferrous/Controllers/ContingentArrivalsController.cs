using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Controllers.Authorization;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/ContingentArrivals")]
    public class ContingentArrivalsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public ContingentArrivalsController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/ContingentArrivals
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public IEnumerable<ContingentArrival> GetContingentArrival()
        {
            return _context.ContingentArrival;
        }

        // GET: api/ContingentArrivals/byCL/5
        [HttpGet("byCL/{clno}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public async Task<IActionResult> GetContingentArrival([FromRoute] string clno)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                .Where(m => m.ContingentLeaderNo == clno)
                .Include(m => m.RoomAllocation)
                .SingleOrDefaultAsync();

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        // GET: api/ContingentArrivals/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<IActionResult> GetContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        // PUT: api/ContingentArrivals/5
        [HttpPut("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> PutContingentArrival([FromRoute] int id, [FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != contingentArrival.ContingentArrivalNo)
            {
                return BadRequest();
            }

            _context.Entry(contingentArrival).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContingentArrivalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ContingentArrivals
        [HttpPost]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_POST)]
        public async Task<IActionResult> PostContingentArrival([FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            _context.ContingentArrival.Add(contingentArrival);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        // DELETE: api/ContingentArrivals/5
        [HttpDelete("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_DELETE)]
        public async Task<IActionResult> DeleteContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);
            if (contingentArrival == null)
            {
                return NotFound();
            }

            _context.ContingentArrival.Remove(contingentArrival);
            await _context.SaveChangesAsync();

            return Ok(contingentArrival);
        }

        private bool ContingentArrivalExists(int id)
        {
            return _context.ContingentArrival.Any(e => e.ContingentArrivalNo == id);
        }
    }
}