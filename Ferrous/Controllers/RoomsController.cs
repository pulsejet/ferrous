using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Controllers.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Rooms")]
    public class RoomsController : ControllerBase
    {
        private static IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        private readonly ferrousContext _context;

        public RoomsController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public static void UpdateLayoutWebSocket(string Building)
        {
            _hubContext.Clients.Group(Building).InvokeAsync("updated", DateTime.Now.ToString());
        }

        // GET: api/Rooms
        [HttpGet]
        public IEnumerable<Room> GetRoom()
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMS_GET))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }
            return _context.Room.Include(m => m.RoomAllocation);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom([FromRoute] int id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_GET_DETAILS))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.Include(m => m.RoomAllocation) 
                                          .SingleOrDefaultAsync(m => m.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        // GET: api/Rooms/ByLoc/H1
        [HttpGet("ByLoc/{loc}")]
        public async Task<IEnumerable<Room>> GetRoomsByLoc([FromRoute] string loc)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOMS_GET))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }

            return await _context.Room.Where(m => m.Location == loc).Include(m => m.RoomAllocation).ToListAsync();
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom([FromRoute] int id, [FromBody] Room room)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_PUT))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != room.RoomId)
            {
                return BadRequest();
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            UpdateLayoutWebSocket(room.Location);
            return NoContent();
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<IActionResult> PostRoom([FromBody] Room room)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_POST))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Room.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.RoomId }, room);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom([FromRoute] int id)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_DELETE))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.SingleOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Room.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Room.Any(e => e.RoomId == id);
        }

        // GET: api/Rooms/allot/CLNo
        [HttpGet("allot/{id}")]
        [HttpGet("allot/{id}/{CLNo}/{CANo}")]
        [HttpGet("allot/{id}/{CLNo}/{CANo}/{partialno}")]
        public async Task<IActionResult> RoomAllot([FromRoute] int id, [FromRoute] string CLNo, [FromRoute] int CANo, [FromRoute] int partialno = -1)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_ALLOT))
                return Unauthorized();

            Room room = await _context.Room.Where(m => m.RoomId == id)
                                    .Include(m => m.RoomAllocation)
                                    .SingleOrDefaultAsync();
            if (room == null) return BadRequest();

            bool partial = partialno > 0;
            bool empty = true;

            foreach (var roomA in room.RoomAllocation)
            {
                if (!(roomA.Partial > 0)) empty = false;
                if (!partial) empty = false;
            }

            if (!empty || room.Status != 1) return BadRequest( "Not Empty");

            RoomAllocation roomAllocation = new RoomAllocation();
            roomAllocation.ContingentLeaderNo = CLNo;
            roomAllocation.ContingentArrivalNo = CANo;
            roomAllocation.RoomId = id;
            roomAllocation.Partial = partialno;
            _context.Update(roomAllocation);

            _context.Update(room);

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(room.Location);

            roomAllocation.Room = null;
            return Ok(roomAllocation);
        }

        // GET: api/Rooms/mark/CLNo
        [HttpGet("mark/{id}/{status}")]
        public async Task<IActionResult> mark([FromRoute] int id, [FromRoute] int status)
        {
            if (!HasPrivilege(User.Identity.Name, 1,
                PrivilegeList.ROOM_MARK))
                return Unauthorized();

            Room room = await _context.Room.Where(m => m.RoomId == id)
                                    .Include(m => m.RoomAllocation)
                                    .SingleOrDefaultAsync();
            if (room == null) return BadRequest();

            if (room.RoomAllocation.Count > 0) return BadRequest("Not Empty");
            room.Status = (short)status;

            _context.Update(room);

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(room.Location);
            return Content("success");
        }

        [HttpGet("CreateRoomRecords/{location}/{start}/{end}/{capacity}")]
        public IActionResult CreateRoomRecords([FromRoute] string location, [FromRoute] int start, [FromRoute] int end, [FromRoute] int capacity)
        {
            if (!HasPrivilege(User.Identity.Name, 0,
                PrivilegeList.ROOM_CREATE))
                return Unauthorized();

            string str = "";
            for (int i = start; i<= end; i++)
            {
                if (_context.Room.Where(m => m.Location == location && m.RoomName == i.ToString()).Count() > 0)
                {
                    str += i.ToString() + " ";
                    continue;
                }
                Room room = new Room();
                room.Location = location;
                room.RoomName = i.ToString();
                room.Capacity = capacity;
                _context.Add(room);
                _context.SaveChanges();
            }
            return Content("Done -- " + str);
        }
    }

}