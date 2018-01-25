using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Buildings")]
    public class BuildingsController : Controller
    {
        private readonly mydbContext _context;

        public BuildingsController(mydbContext context)
        {
            _context = context;
        }

        // GET: api/Buildings
        [HttpGet]
        public IEnumerable<Building> GetBuilding()
        {
            return _context.Building;
        }

        // GET: api/Buildings/e
        [HttpGet("e/{clno}")]
        public async Task<IEnumerable<Building>> GetBuildingExtended([FromRoute] string clno)
        {
            mydbContext ctx = new mydbContext();
            Building[] buildings = await ctx.Building.Include(m => m.Room)
                                            .ThenInclude(m => m.RoomAllocation)
                                            .ToArrayAsync();

            Parallel.ForEach(buildings, building =>
            {
                building.CapacityEmpty = 0;
                foreach (var room in building.Room)
                {
                    if (room.Status == 4) building.CapacityNotReady += room.Capacity;
                    if (room.Status != 1) continue;
                    building.CapacityEmpty += room.Capacity;
                    foreach (var roomA in room.RoomAllocation)
                    {
                        if (roomA.Partial <= 0) {
                            building.CapacityFilled += room.Capacity;
                            building.CapacityEmpty -= room.Capacity;
                            if (roomA.ContingentLeaderNo == clno)
                                building.AlreadyAllocated += room.Capacity;
                            break;
                        }
                        building.CapacityFilled += roomA.Partial;
                        building.CapacityEmpty -= roomA.Partial;
                        if (roomA.ContingentLeaderNo == clno)
                            building.AlreadyAllocated += roomA.Partial;
                    }
                }
                building.Room = null;
            });

            return buildings;
        }

        // GET: api/Buildings/5
        [HttpGet("{id}")]
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