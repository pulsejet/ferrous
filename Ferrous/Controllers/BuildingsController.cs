using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using System;
using Ferrous.Misc;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Buildings")]
    public class BuildingsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public BuildingsController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/Buildings/e
        [HttpGet("e/{id}/{cano}")]
        [HTTPrel(HTTPrelList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDINGS_GET)]
        public async Task<EnumContainer> GetBuildingsExtended([FromRoute] string id, [FromRoute] int cano)
        {
            var buildings = await DataUtilities.GetExtendedBuildings(_context, id);
            foreach (var building in buildings)
                (new LinksMaker(User,Url)).FillBuildingsLinks(building, id, cano);

            return new EnumContainer(
                buildings,
                new LinkHelper()
                .SetOptions(User, this.GetType(), Url)
                .AddLinks(new string[] {
                    nameof(GetBuildingsExtended),
                    nameof(PostBuilding)
                }).GetLinks()
            );
        }

        // GET: api/Buildings/5
        [HttpGet("{id}/{clno}/{cano}")]
        [HTTPrel(HTTPrelList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_GET_DETAILS)]
        public async Task<IActionResult> GetBuilding([FromRoute] string id, [FromRoute] string clno, [FromRoute] int cano)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var building = await _context.Building.Where(m => m.Location == id)
                .Include(m => m.Room)
                .ThenInclude(m => m.RoomAllocation)
                .SingleOrDefaultAsync();

            if (building == null)
            {
                return NotFound();
            } else
            {
                new LinksMaker(User, Url).FillBuildingsLinks(building, clno, cano);
            }

            return Ok(building);
        }

        // PUT: api/Buildings/5
        [HttpPut("{id}")]
        [HTTPrel(HTTPrelList.update)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.BUILDING_PUT)]
        public async Task<IActionResult> PutBuilding([FromRoute] string id, [FromBody] Building building)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != building.Location)
            {
                return BadRequest();
            }

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuildingExists(id))
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

        // POST: api/Buildings
        [HttpPost]
        [HTTPrel(HTTPrelList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_POST)]
        public async Task<IActionResult> PostBuilding([FromBody] Building building)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Building.Add(building);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BuildingExists(building.Location))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBuilding", new { id = building.Location }, building);
        }

        // DELETE: api/Buildings/5
        [HttpDelete("{id}")]
        [HTTPrel(HTTPrelList.delete)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.BUILDING_DELETE)]
        public async Task<IActionResult> DeleteBuilding([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var building = await _context.Building.SingleOrDefaultAsync(m => m.Location == id);
            if (building == null)
            {
                return NotFound();
            }

            _context.Building.Remove(building);
            await _context.SaveChangesAsync();

            return Ok(building);
        }

        private bool BuildingExists(string id)
        {
            return _context.Building.Any(e => e.Location == id);
        }
    }
}