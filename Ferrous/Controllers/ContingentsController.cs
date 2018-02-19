using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Controllers.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Contingents")]
    public class ContingentsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public ContingentsController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/Contingents
        [HttpGet, HTTPrel(HTTPrelList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTS_GET)]
        public async Task<EnumContainer> GetContingents()
        {
            var contingents = await DataUtilities.GetExtendedContingents(_context);

            foreach(var contingent in contingents) {
                new LinksMaker(User, Url).FillContingentsLinks(contingent);
            }

            return new EnumContainer(
                contingents,
                new LinkHelper()
                .SetOptions(User, this.GetType(), Url)
                .AddLinks(new string[] {
                    nameof(GetContingents),
                    nameof(PostContingent)
                }).GetLinks()
            );
        }

        // GET: api/Contingents/5
        [HttpGet("{id}"), HTTPrel(HTTPrelList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTS_GET)]
        public async Task<IActionResult> GetContingent([FromRoute] string id)
        {
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

            new LinksMaker(User, Url).FillContingentsLinks(contingent);

            if (contingent == null) return NotFound();

            foreach ( var ra in contingent.RoomAllocation )
            {
                ra.ContingentArrivalNoNavigation = null;
            }

            return Ok(contingent);
        }

        // PUT: api/Contingents/5
        [HttpPut("{id}"), HTTPrel(HTTPrelList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_PUT)]
        public async Task<IActionResult> PutContingent([FromRoute] string id, [FromBody] Contingents contingents)
        {
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
        [HttpPost, HTTPrel(HTTPrelList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_POST)]
        public async Task<IActionResult> PostContingent([FromBody] Contingents contingents)
        {
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
        [HttpDelete("{id}"), HTTPrel(HTTPrelList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_DELETE)]
        public async Task<IActionResult> DeleteContingent([FromRoute] string id)
        {
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