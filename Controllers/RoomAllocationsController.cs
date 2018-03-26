using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;
using Microsoft.AspNetCore.SignalR;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/RoomAllocations")]
    public class RoomAllocationsController : ControllerBase
    {
        private readonly ferrousContext _context;
        private readonly IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        public RoomAllocationsController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public void UpdateLayoutWebSocket(Room[] rooms)
        {
            if (rooms.Length == 0) { return; }
            _hubContext.Clients.Group(rooms[0].Location).SendAsync("updated", rooms.Select(r => r.RoomId).ToArray());
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
        [LinkRelation(LinkRelationList.update)]
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
        [LinkRelation(LinkRelationList.create)]
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
        [LinkRelation(LinkRelationList.delete)]
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

            UpdateLayoutWebSocket(new Room[] {roomAllocation.Room});

            return NoContent();
        }

        private bool RoomAllocationExists(int id)
        {
            return _context.RoomAllocation.Any(e => e.Sno == id);
        }
    }
}