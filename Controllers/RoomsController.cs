using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using Microsoft.AspNetCore.SignalR;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        private readonly ferrousContext _context;

        public RoomsController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public void UpdateLayoutWebSocket(Room[] rooms)
        {
            if (rooms.Length == 0) { return; }
            _hubContext.Clients.Group(rooms[0].Location).SendAsync("updated", rooms.Select(r => r.RoomId).ToArray());
        }

        // GET: api/Rooms/5
        [HttpGet("{id}/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMS_GET)]
        public async Task<IActionResult> GetRoom([FromRoute] int id, [FromRoute] string clno, [FromRoute] int cano)
        {
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

            new LinksMaker(User, Url).FillRoomLinks(room, clno, cano);

            return Ok(room);
        }

        // POST: api/Rooms/list/clno/cano (gets list of rooms)
        [HttpPost("list/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMS_GET)]
        public async Task<ActionResult> GetRoomList([FromRoute] string clno, [FromRoute] int cano, [FromBody] List<int> ids)
        {
            var rooms = await _context.Room.Include(m => m.RoomAllocation)
                                          .Where(m => ids.Contains(m.RoomId))
                                          .ToListAsync();

            if (rooms == null)
            {
                return NotFound();
            }

            foreach (var room in rooms) {
                new LinksMaker(User, Url).FillRoomLinks(room, clno, cano);
            }

            return Ok(rooms);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        [LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_PUT)]
        public async Task<IActionResult> PutRoom([FromRoute] int id, [FromBody] Room room)
        {
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

            UpdateLayoutWebSocket(new Room[] {room});
            return NoContent();
        }

        // POST: api/Rooms
        [HttpPost]
        [LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_POST)]
        public async Task<IActionResult> PostRoom([FromBody] Room room)
        {
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
        [LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_DELETE)]
        public async Task<IActionResult> DeleteRoom([FromRoute] int id)
        {
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

        // POST: api/Rooms/allot/CLNo/CANo
        [HttpPost("allot/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_ALLOT)]
        public async Task<IActionResult> AllotRooms([FromBody] RoomAllocation[] roomAllocations, [FromRoute] string clno, [FromRoute] int cano)
        {
            var ids = roomAllocations.Select(ra => ra.RoomId).ToArray();
            Room[] rooms = await _context.Room.Where(m => ids.Contains(m.RoomId))
                                    .Include(m => m.RoomAllocation)
                                    .ToArrayAsync();
            if (rooms == null) { return BadRequest(); }

            foreach (var room in rooms) {
                var NewRoomA = roomAllocations.Where(ra => ra.RoomId == room.RoomId).SingleOrDefault();
                bool partial = NewRoomA.Partial > 0;
                bool empty = true;

                foreach (var roomA in room.RoomAllocation)
                {
                    if (roomA.Partial <= 0) { empty = false; }
                    if (!partial) { empty = false; }
                }

                if (!empty || room.Status != 1) { continue; }

                if (!partial) { NewRoomA.Partial = -1; }

                NewRoomA.ContingentLeaderNo = clno;
                NewRoomA.ContingentArrivalNo = cano;

                _context.Update(NewRoomA);
                _context.Update(room);
            }

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(rooms);

            return NoContent();
        }

        // POST: api/Rooms/mark
        [HttpPost("mark")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_MARK)]
        public async Task<ActionResult> MarkRooms([FromBody] List<int> ids, [FromQuery] int status)
        {
            var rooms = await _context.Room.Where(m => ids.Contains(m.RoomId))
                                    .Include(m => m.RoomAllocation)
                                    .ToListAsync();
            if (rooms == null) return BadRequest();

            foreach(var room in rooms) {
                if (room.RoomAllocation.Count > 0) { continue; }
                room.Status = (short)status;
                _context.Update(room);
            }

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(rooms.ToArray());
            return NoContent();
        }

        [HttpGet("CreateRoomRecords/{location}/{start}/{end}/{capacity}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_CREATE)]
        public IActionResult CreateRoomRecords([FromRoute] string location, [FromRoute] int start, [FromRoute] int end, [FromRoute] int capacity)
        {
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