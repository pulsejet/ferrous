using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using System.Security.Claims;
using Ferrous.Misc;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/ContingentArrivals")]
    public class ContingentArrivalsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public ContingentArrivalsController(ferrousContext context)
        {
            _context = context;
        }

        // DEPRECATED
        // GET: api/ContingentArrivals
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public IEnumerable<ContingentArrival> GetContingentArrival()
        {
            return _context.ContingentArrival;
        }

        // DEPRECATED
        // GET: api/ContingentArrivals/byCL/5
        [HttpGet("byCL/{clno}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public async Task<IActionResult> GetContingentArrival([FromRoute] string clno)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                .Where(m => m.ContingentLeaderNo == clno)
                .Include(m => m.RoomAllocation)
                .SingleOrDefaultAsync();

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        // GET: api/ContingentArrivals/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<IActionResult> GetContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        [HttpPut("desk1/approve/{cano}"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> ApproveContingentArrival([FromRoute] int cano, [FromBody] ContingentArrival contingentArrivalPut)
        {
            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.CAPeople)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == cano);
            if (contingentArrival == null)
            {
                return NotFound();
            }

            /* Set M/F count from posted data */
            contingentArrival.Male = contingentArrivalPut.Male;
            contingentArrival.Female = contingentArrivalPut.Female;

            /* Mark people as done with */
            foreach (CAPerson caPerson in contingentArrival.CAPeople) {
                Person person = await _context.Person.SingleOrDefaultAsync(m => m.Mino == caPerson.Mino);
                if (person != null) {
                    person.allottedCA = contingentArrival;
                }
            }

            /* Approve! */
            contingentArrival.Approved = true;

            await _context.SaveChangesAsync();

            return Ok(contingentArrival);
        }

        // GET: api/ContingentArrivals/desk1?id=5
        [HttpGet("desk1"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<IActionResult> GetDesk1([FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.CAPeople)
                                                .Include(m => m.RoomAllocation)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            List<string> minos = contingentArrival.CAPeople.Select(m => m.Mino).ToList();
            Person[] people = await _context.Person.Where(m => minos.Contains(m.Mino))
                                                    .Include(m => m.allottedCA)
                                                    .ToArrayAsync();

            foreach (CAPerson caPerson in contingentArrival.CAPeople) {
                FillCAPerson(caPerson, people, contingentArrival.ContingentLeaderNo);
            }

            await _context.SaveChangesAsync();

            contingentArrival.PeopleFemale = contingentArrival.CAPeople.Count(m => m.Sex.ToUpper() == "F");
            contingentArrival.PeopleMale = contingentArrival.CAPeople.Count(m => m.Sex.ToUpper() == "M");

            new LinksMaker(User, Url).FillContingentArrivalLinks(contingentArrival);

            return Ok(contingentArrival);
        }

        public void FillCAPerson(CAPerson caPerson, Person[] people, string clno, bool links = true) {
            if (links) {
                new LinksMaker(User, Url).FillCAPersonLinks(caPerson);
            }
            var person = people.SingleOrDefault(m => m.Mino == caPerson.Mino);
            if (person != null) {
                caPerson.person = person;

                if (links) {
                    new LinksMaker(User, Url).FillPersonLinks(person);
                }

                caPerson.Sex = person.Sex;
                if (person.ContingentLeaderNo != clno) {
                    // Bad Contingent Leader
                    caPerson.flags += "BCL";
                }
                if (person.allottedCA != null) {
                    // Person already approved (in another subcontingent etc)
                    caPerson.flags += "PAA";
                }
            } else {
                // No Registered Person
                caPerson.flags += "NRP";
                caPerson.Sex = "?";
            }
        }

        // POST: api/ContingentArrivals/desk1/cap?id=5
        [HttpPost("desk1/cap/{cano}"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> PostCAPerson([FromRoute] int cano, [FromBody] CAPerson caPerson)
        {
            caPerson.CANav = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == cano);
            _context.CAPerson.Add(caPerson);
            await _context.SaveChangesAsync();

            Person[] people = await _context.Person.Where(m => m.Mino == caPerson.Mino).ToArrayAsync();
            FillCAPerson(caPerson, people, caPerson.CANav.ContingentLeaderNo);

            return CreatedAtAction("PostCAPerson", new { id = caPerson.Sno }, caPerson);
        }

        // DELETE: api/ContingentArrivals/desk1/5
        [HttpDelete("desk1/cap/{id}"), LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> DeleteCAPerson([FromRoute] int id)
        {
            var caPerson = await _context.CAPerson.SingleOrDefaultAsync(m => m.Sno == id);
            if (caPerson == null)
            {
                return NotFound();
            }

            _context.CAPerson.Remove(caPerson);
            await _context.SaveChangesAsync();

            return Ok(caPerson);
        }

        // PUT: api/ContingentArrivals/5
        [HttpPut("{id}")]
        [LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> PutContingentArrival([FromRoute] int id, [FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != contingentArrival.ContingentArrivalNo)
            {
                return BadRequest();
            }

            _context.Entry(contingentArrival).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContingentArrivalExists(id))
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

        // POST: api/ContingentArrivals
        [HttpPost]
        [LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_POST)]
        public async Task<IActionResult> PostContingentArrival([FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            _context.ContingentArrival.Add(contingentArrival);
            await _context.SaveChangesAsync();

            new LinksMaker(User, Url).FillContingentArrivalLinks(contingentArrival);

            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        // DELETE: api/ContingentArrivals/5
        [HttpDelete("{id}")]
        [LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_DELETE)]
        public async Task<IActionResult> DeleteContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);
            if (contingentArrival == null)
            {
                return NotFound();
            }

            _context.ContingentArrival.Remove(contingentArrival);
            await _context.SaveChangesAsync();

            return Ok(contingentArrival);
        }

        private bool ContingentArrivalExists(int id)
        {
            return _context.ContingentArrival.Any(e => e.ContingentArrivalNo == id);
        }
    }
}