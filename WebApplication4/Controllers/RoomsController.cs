using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunk_Ferrous.Models;

namespace WebApplication4.Controllers
{
    [Produces("application/json")]
    [Route("api/Rooms")]
    public class RoomsController : Controller
    {
        private readonly mydbContext _context;

        public RoomsController(mydbContext context)
        {
            _context = context;
        }

        // GET: api/Rooms
        [HttpGet]
        public IEnumerable<Room> GetRoom()
        {
            return _context.Room.Include(m => m.RoomAllocation);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.Include(m => m.RoomAllocation) 
                                          .SingleOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        // GET: api/Rooms/ByLoc/H1
        [HttpGet("ByLoc/{loc}")]
        public IEnumerable<Room> GetRoomsByLoc([FromRoute] string loc)
        {
            return _context.Room.Where(m => m.Location == loc).Include(m => m.RoomAllocation);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom([FromRoute] int id, [FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != room.Id)
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

            return NoContent();
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<IActionResult> PostRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Room.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.Id }, room);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.SingleOrDefaultAsync(m => m.Id == id);
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
            return _context.Room.Any(e => e.Id == id);
        }

        // GET: api/Rooms/allot/CLNo
        [HttpGet("allot/{id}")]
        [HttpGet("allot/{id}/{CLNo}")]
        public async Task<IActionResult> RoomAllot([FromRoute] int id, [FromRoute] string CLNo)
        {
            Room room = _context.Room.Where(m => m.Id == id)
                                    .Include(m => m.RoomAllocation)
                                    .SingleOrDefault();
            if (room == null) return BadRequest();

            bool empty = true;
            foreach (var roomA in room.RoomAllocation)
            {
                if (!(bool)roomA.Partial) empty = false;
            }

            if (!empty || room.Status != 1) return BadRequest( "Not Empty");

            RoomAllocation roomAllocation = new RoomAllocation();
            roomAllocation.ContingentLeaderNo = CLNo;
            roomAllocation.RoomId = id;
            roomAllocation.Partial = false;
            _context.Update(roomAllocation);

            _context.Update(room);

            await _context.SaveChangesAsync();
            return Content("success");
        }

        // GET: api/Rooms/mark/CLNo
        [HttpGet("mark/{id}/{status}")]
        public async Task<IActionResult> mark([FromRoute] int id, [FromRoute] int status)
        {
            Room room = _context.Room.Where(m => m.Id == id)
                                    .Include(m => m.RoomAllocation)
                                    .SingleOrDefault();
            if (room == null) return BadRequest();

            if (room.RoomAllocation.Count > 0) return BadRequest("Not Empty");
            room.Status = (short)status;

            _context.Update(room);

            await _context.SaveChangesAsync();
            return Content("success");
        }
    }

}