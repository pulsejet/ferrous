using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Controllers.Authorization;

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

        // GET: api/Buildings
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDINGS_GET)]
        public IEnumerable<Building> GetBuilding()
        {
            return _context.Building;
        }

        // GET: api/Buildings/e
        [HttpGet("e/{id}")][HTTPrel(HTTPrelList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_GET_DETAILS)]
        public async Task<IEnumerable<Building>> GetBuildingExtended([FromRoute] string id)
        {
            return await DataUtilities.GetExtendedBuildings(_context, id);
        }

        // GET: api/Buildings/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_GET_DETAILS)]
        public async Task<IActionResult> GetBuilding([FromRoute] string id)
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
            }

            return Ok(building);
        }

        // PUT: api/Buildings/5
        [HttpPut("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_PUT)]
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
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_DELETE)]
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