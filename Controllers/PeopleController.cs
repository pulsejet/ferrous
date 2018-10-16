using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;
using System.IO;

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
        [HttpGet][LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PEOPLE_GET)]
        public EnumContainer GetPeople()
        {
            var people = _context.Person;
            foreach (var person in people)
            {
                new LinksMaker(User, Url).FillPersonLinks(person);
            }

            return new EnumContainer(
                people,
                new LinkHelper()
                .SetOptions(User, this.GetType(), Url)
                .AddLink(nameof(GetPeople))
                .AddLink(nameof(PostPerson))
                .GetLinks()
            );
        }

        // GET: api/People/5
        [HttpGet("{id}"), LinkRelation(LinkRelationList.self)]
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
            } else
            {
                new LinksMaker(User, Url).FillPersonLinks(person);
            }

            return Ok(person);
        }

        // PUT: api/People/5
        [HttpPut("{id}")][LinkRelation(LinkRelationList.update)]
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
        [HttpPost][LinkRelation(LinkRelationList.create)]
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
        [HttpDelete("{id}")][LinkRelation(LinkRelationList.delete)]
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

        [HttpGet("forward"), LinkRelation(LinkRelationList.overridden)]
        [Misc.Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_GET_DETAILS)]
        public async Task<IActionResult> GetPersonForward([FromQuery] string id){
            var httpWebRequest = (System.Net.HttpWebRequest) System.Net.WebRequest.Create("https://api2.moodi.org/user/" + id);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                string json = "";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                var result = streamReader.ReadToEnd();
                return Content(result, "application/json");
            }
        }
    }
}