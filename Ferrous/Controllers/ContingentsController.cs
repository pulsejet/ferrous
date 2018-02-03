using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Utilities;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ContingentsController : ControllerBase
    {
        private readonly mydbContext _context;

        public ContingentsController(mydbContext context)
        {
            _context = context;
        }

        // GET: api/Contingents
        [HttpGet]
        public async Task<IEnumerable<Contingents>> GetContingents()
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.CONTINGENTS_GET))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }

            Contingents[] cts = await _context.Contingents
                                        .Include(m => m.Person)
                                        .Include(m => m.ContingentArrival).ToArrayAsync();

            foreach (var contingent in cts)
            {
                int RegMale = 0, RegFemale = 0, 
                    ArrivedM = 0, ArrivedF = 0, 
                    OnSpotM = 0, OnSpotF = 0;

                Parallel.ForEach(contingent.Person, person => {
                    if (person == null) return;
                    if (person.Sex == "M") RegMale += 1;
                    else if (person.Sex == "F") RegFemale += 1;
                });

                Parallel.ForEach(contingent.ContingentArrival, ca =>
                {
                    ArrivedM += dbCInt(ca.Male);
                    ArrivedF += dbCInt(ca.Female);
                    OnSpotM += dbCInt(ca.MaleOnSpot);
                    OnSpotF += dbCInt(ca.FemaleOnSpot);
                });

                contingent.Male = RegMale.ToString();
                contingent.Female = RegFemale.ToString();
                contingent.ArrivedM = ArrivedM.ToString() + ((OnSpotM > 0) ? " + " + OnSpotM : "");
                contingent.ArrivedF = ArrivedF.ToString() + ((OnSpotF > 0) ? " + " + OnSpotF : "");
            }

            return _context.Contingents;
        }

        // GET: api/Contingents/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContingents([FromRoute] string id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                Utilities.PrivilegeList.CONTINGENT_GET_DETAILS))
                return Unauthorized();

                if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Contingents contingent = await _context.Contingents.Where(m => m.ContingentLeaderNo == id)
                                            .Include(m => m.RoomAllocation)
                                                .ThenInclude(m => m.Room)
                                            .Include(m => m.Person)
                                            .Include(m => m.ContingentArrival)
                                            .SingleOrDefaultAsync();

            foreach ( var ra in contingent.RoomAllocation )
            {
                ra.ContingentArrivalNoNavigation = null;
            }

            if (contingent == null)
            {
                return NotFound();
            }

            return Ok(contingent);
        }

        // PUT: api/Contingents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContingents([FromRoute] string id, [FromBody] Contingents contingents)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.CONTINGENT_PUT))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != contingents.ContingentLeaderNo)
            {
                return BadRequest();
            }

            _context.Entry(contingents).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContingentsExists(id))
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

        // POST: api/Contingents
        [HttpPost]
        public async Task<IActionResult> PostContingents([FromBody] Contingents contingents)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                Utilities.PrivilegeList.CONTINGENT_POST))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Contingents.Add(contingents);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ContingentsExists(contingents.ContingentLeaderNo))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetContingents", new { id = contingents.ContingentLeaderNo }, contingents);
        }

        // DELETE: api/Contingents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContingents([FromRoute] string id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                Utilities.PrivilegeList.CONTINGENT_DELETE))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingents = await _context.Contingents.SingleOrDefaultAsync(m => m.ContingentLeaderNo == id);
            if (contingents == null)
            {
                return NotFound();
            }

            _context.Contingents.Remove(contingents);
            await _context.SaveChangesAsync();

            return Ok(contingents);
        }

        private bool ContingentsExists(string id)
        {
            return _context.Contingents.Any(e => e.ContingentLeaderNo == id);
        }
    }
}