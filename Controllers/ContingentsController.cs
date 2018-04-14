using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using Ferrous.Misc;

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
        [HttpGet, LinkRelation(LinkRelationList.self)]
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
                .AddLink(nameof(GetContingents))
                .AddLink(nameof(PostContingent))
                .GetLinks()
            );
        }

        // GET: api/Contingents?5
        [HttpGet("find"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_GET_DETAILS)]
        public async Task<IActionResult> FindContingent([FromQuery] string id) {
            return await GetContingent(id).ConfigureAwait(false);
        }

        // GET: api/Contingents/5
        [HttpGet("{id}"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_GET_DETAILS)]
        public async Task<IActionResult> GetContingent([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Contingent contingent = await _context.Contingents.Where(m => m.ContingentLeaderNo == id)
                                            .Include(m => m.RoomAllocation)
                                                .ThenInclude(m => m.Room)
                                            .Include(m => m.Person)
                                            .Include(m => m.ContingentArrival)
                                            .SingleOrDefaultAsync();

            if (contingent == null) {
                return NotFound();
            }

            new LinksMaker(User, Url).FillContingentsLinks(contingent);

            return Ok(contingent);
        }

        // PUT: api/Contingents/5
        [HttpPut("{id}"), LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_PUT)]
        public async Task<IActionResult> PutContingent([FromRoute] string id, [FromBody] Contingent contingents)
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
        [HttpPost, LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENT_POST)]
        public async Task<IActionResult> PostContingent([FromBody] Contingent contingents)
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
        [HttpDelete("{id}"), LinkRelation(LinkRelationList.delete)]
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