using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/RoomAllocations")]
    public class RoomAllocationsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public RoomAllocationsController(ferrousContext context)
        {
            _context = context;
        }

        // DEPRECATED
        // GET: api/RoomAllocations
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMALLOCATIONS_GET)]
        public IEnumerable<RoomAllocation> GetRoomAllocation()
        {
            return _context.RoomAllocation;
        }

        // DEPRECATED
        // GET: api/RoomAllocations/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMALLOCATIONS_GET_DETAILS)]
        public async Task<IActionResult> GetRoomAllocation([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roomAllocation = await _context.RoomAllocation.SingleOrDefaultAsync(m => m.Sno == id);

            if (roomAllocation == null)
            {
                return NotFound();
            }

            return Ok(roomAllocation);
        }

        // PUT: api/RoomAllocations/5
        [HttpPut("{id}")]
        [HTTPrel(HTTPrelList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMALLOCATIONS_PUT)]
        public async Task<IActionResult> PutRoomAllocation([FromRoute] int id, [FromBody] RoomAllocation roomAllocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != roomAllocation.Sno)
            {
                return BadRequest();
            }

            _context.Entry(roomAllocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomAllocationExists(id))
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

        // POST: api/RoomAllocations
        [HttpPost]
        [HTTPrel(HTTPrelList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMALLOCATIONS_POST)]
        public async Task<IActionResult> PostRoomAllocation([FromBody] RoomAllocation roomAllocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.RoomAllocation.Add(roomAllocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoomAllocation", new { id = roomAllocation.Sno }, roomAllocation);
        }

        // DELETE: api/RoomAllocations/5
        [HttpDelete("{id}")]
        [HTTPrel(HTTPrelList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMALLOCATIONS_DELETE)]
        public async Task<IActionResult> DeleteRoomAllocation([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roomAllocation = await _context.RoomAllocation.Where(m => m.Sno == id)
                                            .Include(m => m.Room)
                                            .SingleOrDefaultAsync();
            if (roomAllocation == null)
            {
                return NotFound();
            }

            _context.RoomAllocation.Remove(roomAllocation);
            await _context.SaveChangesAsync();

            RoomsController.UpdateLayoutWebSocket(roomAllocation.Room.Location);

            return Ok(roomAllocation);
        }

        private bool RoomAllocationExists(int id)
        {
            return _context.RoomAllocation.Any(e => e.Sno == id);
        }
    }
}