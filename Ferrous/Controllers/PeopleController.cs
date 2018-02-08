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
    [Route("api/People")]
    public class PeopleController : ControllerBase
    {
        private readonly ferrousContext _context;

        public PeopleController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PEOPLE_GET)]
        public IEnumerable<Person> GetPerson()
        {
            return _context.Person;
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_GET_DETAILS)]
        public async Task<IActionResult> GetPerson([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.SingleOrDefaultAsync(m => m.Mino == id);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        // PUT: api/People/5
        [HttpPut("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_PUT)]
        public async Task<IActionResult> PutPerson([FromRoute] string id, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != person.Mino)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
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

        // POST: api/People
        [HttpPost]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_POST)]
        public async Task<IActionResult> PostPerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Person.Add(person);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PersonExists(person.Mino))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPerson", new { id = person.Mino }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_DELETE)]
        public async Task<IActionResult> DeletePerson([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.SingleOrDefaultAsync(m => m.Mino == id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            return Ok(person);
        }

        private bool PersonExists(string id)
        {
            return _context.Person.Any(e => e.Mino == id);
        }
    }
}