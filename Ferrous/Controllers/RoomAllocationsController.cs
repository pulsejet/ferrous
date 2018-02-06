using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Utilities;

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

        // GET: api/RoomAllocations
        [HttpGet]
        public IEnumerable<RoomAllocation> GetRoomAllocation()
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMALLOCATIONS_GET))
            {
                Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                return null;
            }
            return _context.RoomAllocation;
        }

        // GET: api/RoomAllocations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomAllocation([FromRoute] int id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMALLOCATIONS_GET_DETAILS))
                return Unauthorized();

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
        public async Task<IActionResult> PutRoomAllocation([FromRoute] int id, [FromBody] RoomAllocation roomAllocation)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMALLOCATIONS_PUT))
                return Unauthorized();

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
        public async Task<IActionResult> PostRoomAllocation([FromBody] RoomAllocation roomAllocation)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMALLOCATIONS_POST))
                return Unauthorized();

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
        public async Task<IActionResult> DeleteRoomAllocation([FromRoute] int id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMALLOCATIONS_DELETE))
                return Unauthorized();

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